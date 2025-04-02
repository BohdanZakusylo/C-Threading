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

// Include the namespace where the map resides

public class CsvService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CsvService> _logger;
    private const int BatchSize = 1000;
    
    //dynamically determine parallelism
    private readonly int _maxDegreeOfParallelism;
    
    public CsvService(IDbContextFactory<AppDbContext> contextFactory, ILogger<CsvService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        
        _maxDegreeOfParallelism = Math.Min(8, Math.Max(2, (int)(Environment.ProcessorCount * 0.7)));
        _logger.LogInformation($"CSV Service initialized with parallelism degree: {_maxDegreeOfParallelism}");
    }

    public async Task<IEnumerable<Airbnb>> ReadAirbnbCsvFile(Stream fileStream)
    {
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture);

            using (var reader = new StreamReader(fileStream))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<AirbnbMap>(); // Register the custom map

                // Read all records into memory first
                _logger.LogInformation("Reading CSV records into memory");
                
                // Properly collect async records
                var allRecords = new List<Airbnb>();
                await foreach (var record in csv.GetRecordsAsync<Airbnb>())
                {
                    allRecords.Add(record);
                }
                
                _logger.LogInformation($"Read {allRecords.Count} records from CSV");

                // Create batches for parallel processing
                var batches = CreateBatch(allRecords, BatchSize).ToList();
                _logger.LogInformation($"Created {batches.Count} batches for processing");

                // Instead of Parallel.ForEachAsync, use Task-based approach
                var tasks = new List<Task>();
                var results = new ConcurrentBag<Airbnb>();
            
                foreach (var batch in batches)
                {
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
                    });
                
                    tasks.Add(task);
                
                    // Optional: Limit concurrent tasks
                    if (tasks.Count >= _maxDegreeOfParallelism)
                    {
                        await Task.WhenAny(tasks);
                        tasks.RemoveAll(t => t.IsCompleted);
                    }
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

    private IEnumerable<List<Airbnb>> CreateBatch(List<Airbnb> allRecords, int batchSize)
    {
        for (int i = 0; i < allRecords.Count; i += batchSize)
        {
            yield return allRecords.Skip(i).Take(batchSize).ToList();
        }
    }

    private async Task SaveBatchAsync(List<Airbnb> batch)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var uniqueBatch = batch.GroupBy(f => f.Id).Select(g => g.First()).ToList();
                    var batchIds = uniqueBatch.Select(f => f.Id).ToList();
                    var existingFlights = await context.Airbnbs.Where(f => batchIds.Contains(f.Id)).ToListAsync();
                    
                    var newRecords = uniqueBatch
                        .Where(f => !existingFlights.Any(existing => existing.Id == f.Id))
                        .ToList();

                    if (newRecords.Any())
                    {
                        await context.Airbnbs.AddRangeAsync(newRecords);
                        await context.SaveChangesAsync();
                    }
                    
                    await transaction.CommitAsync();
                    _logger.LogInformation($"Saved {newRecords.Count} records to database");
                }
                catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    _logger.LogError(ex, "Duplicate key value violates unique constraint 'PK_Airbnbs'");
                    await transaction.RollbackAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving batch to database");
                    await transaction.RollbackAsync();
                    throw new ApplicationException("Error saving batch to database", ex);
                }
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