using System.Collections.Concurrent;
using System.Globalization;
using ABMB.Models;
using ABMB.Properties;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Linq;
using System.Threading;

namespace ABMB.Controllers
{
    public class AirbnbCsvService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<AirbnbCsvService> _logger;
        private const int BatchSize = 1000;
        private static readonly object _lockObject = new();
        private readonly SemaphoreSlim _semaphore;
        
        //dynamically determine parallelism
        private readonly int _maxDegreeOfParallelism;
        
        public AirbnbCsvService(IDbContextFactory<AppDbContext> contextFactory, ILogger<AirbnbCsvService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            
            _maxDegreeOfParallelism = Math.Min(8, Math.Max(2, (int)(Environment.ProcessorCount * 0.7)));
            _semaphore = new SemaphoreSlim(_maxDegreeOfParallelism, _maxDegreeOfParallelism);
            _logger.LogInformation($"CSV Service initialized with parallelism degree: {_maxDegreeOfParallelism}");
        }

        public async Task<IEnumerable<Airbnb>> ReadAirbnbCsvFile(Stream fileStream)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture);

                using (var reader = new StreamReader(fileStream))
                using (var airbnbCsv = new CsvReader(reader, config))
                {
                    airbnbCsv.Context.RegisterClassMap<AirbnbMap>(); // Register the custom map

                    // Read all records into memory first
                    _logger.LogInformation("Reading CSV records into memory");
                    
                    // Properly collect async records
                    var allRecords = new List<Airbnb>();
                    await foreach (var record in airbnbCsv.GetRecordsAsync<Airbnb>())
                    {
                        allRecords.Add(record);
                    }
                    
                    _logger.LogInformation($"Read {allRecords.Count} records from CSV");

                    // Create batches for parallel processing
                    var batches = CreateBatch(allRecords, BatchSize).ToList();
                    _logger.LogInformation($"Created {batches.Count} batches for processing");

                    // Instead of Parallel.ForEachAsync, use Task-based approach
                    var results = new ConcurrentBag<Airbnb>();
                    var tasks = new List<Task>();
                
                    foreach (var batch in batches)
                    {
                        await _semaphore.WaitAsync();
                        var task = Task.Run(async () => 
                        {
                            try
                            {
                                await SaveBatchAsync(batch);
                                foreach (var record in batch)
                                {
                                    results.Add(record);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing batch with task");
                            }
                            finally
                            {
                                _semaphore.Release();
                            }
                        });
                    
                        tasks.Add(task);
                    }
                
                    // Wait for all remaining tasks to complete
                    await Task.WhenAll(tasks);
                    _logger.LogInformation($"Completed task-based processing, saved {results.Count} records");
                    return results;
                }
            }
            catch (HeaderValidationException e)
            {
                _logger.LogError(e, "CSV file header is invalid");
                throw new ApplicationException("CSV file header is invalid.", e);
            }
            catch (TypeConverterException ex)
            {
                _logger.LogError(ex, "CSV file contains invalid data format");
                throw new ApplicationException("CSV file contains invalid data format.", ex);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error reading CSV file");
                throw new ApplicationException("Error reading CSV file", e);
            }
        }

        private Task SaveBatchAsync(List<Airbnb> batch)
        {
            var uniqueBatch = batch.GroupBy(f => f.Id).Select(g => g.First()).ToList();

            lock (_lockObject)
            {
                using var context = _contextFactory.CreateDbContext();
                using var transaction = context.Database.BeginTransaction();

                try
                {
                    var batchIds = uniqueBatch.Select(f => f.Id);
                    var existingAirbnbs = context.Airbnbs
                        .Where(f => batchIds.Contains(f.Id))
                        .ToList();

                    var newRecords = uniqueBatch
                        .Where(f => !existingAirbnbs.Any(existing => existing.Id == f.Id))
                        .ToList();

                    if (newRecords.Count > 0)
                    {
                        context.Airbnbs.AddRange(newRecords);
                        context.SaveChanges();
                    }

                    transaction.Commit();
                    _logger.LogInformation($"Saved {newRecords.Count} new records to database");
                    return Task.CompletedTask;
                }
                catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    _logger.LogError(ex, "Duplicate key value violates unique constraint 'PK_Airbnbs'");
                    context.Database.RollbackTransaction();
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving batch to database");
                    context.Database.RollbackTransaction();
                    throw new ApplicationException("Error saving batch to database", ex);
                }
            }
        }

        private static IEnumerable<List<Airbnb>> CreateBatch(List<Airbnb> allRecords, int batchSize)
        {
            for (int i = 0; i < allRecords.Count; i += batchSize)
            {
                yield return allRecords.Skip(i).Take(batchSize).ToList();
            }
        }
    }
}

// using System.Collections.Generic;
// using System.Threading.Tasks;
// using ABMB.Models;
// using Microsoft.Extensions.Configuration;

// namespace ABMB.Controllers
// {
//     public class CsvService
//     {
//         private readonly CsvFileReader _csvReader;

//         public CsvService(IConfiguration configuration)
//         {
//             var filePath = configuration.GetValue<string>("CsvSettings:FilePath"); // Read from appsettings.json
//             _csvReader = new CsvFileReader(filePath);
//         }

//         public async Task<IEnumerable<OldFlight>> ReadFlightCsvFileAsync()
//         {
//             return await Task.Run(() => _csvReader.ReadFlightCsvFile());
//         }

//          public async Task<IEnumerable<Airbnb>> ReadAirbnbCsvFileAsync()
//         {
//             return await Task.Run(() => _csvReader.ReadAirbnbCsvFile());
//         }
//     }
// }