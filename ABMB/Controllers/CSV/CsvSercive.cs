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

    public async Task<IEnumerable<OldFlight>> ReadCsvFile(Stream fileStream)
{
    try
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture);

        using (var reader = new StreamReader(fileStream))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<OldFlightMap>(); // Register the custom map

            // Read all records into memory first
            _logger.LogInformation("Reading CSV records into memory");

            var allRecords = new List<OldFlight>();
            await foreach (var record in csv.GetRecordsAsync<OldFlight>())
            {
                allRecords.Add(record);
            }

            _logger.LogInformation($"Read {allRecords.Count} records from CSV");

            // Create batches for parallel processing
            var batches = CreateBatch(allRecords, BatchSize).ToList();
            _logger.LogInformation($"Created {batches.Count} batches for processing");

            // Use PLINQ to process batches in parallel
            var results = new ConcurrentBag<OldFlight>();

            batches.AsParallel()
                   .WithDegreeOfParallelism(_maxDegreeOfParallelism)
                   .ForAll(batch =>
                   {
                       try
                       {
                           SaveBatchAsync(batch).Wait(); // Wait synchronously for batch save
                           foreach (var record in batch)
                           {
                               results.Add(record);
                           }
                       }
                       catch (Exception ex)
                       {
                           _logger.LogError(ex, "Error processing batch with PLINQ");
                       }
                   });

            _logger.LogInformation($"Completed PLINQ processing, saved {results.Count} records");
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

    private IEnumerable<List<OldFlight>> CreateBatch(List<OldFlight> allRecords, int batchSize)
    {
        for (int i = 0; i < allRecords.Count; i += batchSize)
        {
            yield return allRecords.Skip(i).Take(batchSize).ToList();
        }
    }

    private async Task SaveBatchAsync(List<OldFlight> batch)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var uniqueBatch = batch.GroupBy(f => f.Id).Select(g => g.First()).ToList();
                    var batchIds = uniqueBatch.Select(f => f.Id).ToList();
                    var existingFlights = await context.OldFlights.Where(f => batchIds.Contains(f.Id)).ToListAsync();
                    
                    var newRecords = uniqueBatch
                        .Where(f => !existingFlights.Any(existing => existing.Id == f.Id))
                        .ToList();

                    if (newRecords.Any())
                    {
                        await context.OldFlights.AddRangeAsync(newRecords);
                        await context.SaveChangesAsync();
                    }
                    
                    await transaction.CommitAsync();
                    _logger.LogInformation($"Saved {newRecords.Count} records to database");
                }
                catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    _logger.LogError(ex, "Duplicate key value violates unique constraint 'PK_OldFlights'");
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