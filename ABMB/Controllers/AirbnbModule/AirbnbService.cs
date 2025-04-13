using System.Globalization;
using ABMB.Properties;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using ABMB.Models;
using CsvHelper.TypeConversion;
using System.Text;


public class AirbnbService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<AirbnbService> _logger;

     // Static lock object shared across all instances
    private static readonly object _saveLock = new();

    public AirbnbService(IDbContextFactory<AppDbContext> contextFactory, ILogger<AirbnbService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Airbnb>> ReadCsvFileAirbnb(Stream fileStream)
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

                var allRecords = new List<Airbnb>();
                await foreach (var record in csv.GetRecordsAsync<Airbnb>())
                {
                    allRecords.Add(record);
                }

                _logger.LogInformation($"Read {allRecords.Count} records from CSV");

                lock(_saveLock){
                    _logger.LogInformation("Acquired lock for saving records");
                    // Wait synchronously inside the lock to avoid race conditions
                    SaveAllRecordsAsync(allRecords).GetAwaiter().GetResult();
                }

                _logger.LogInformation("All records have been saved to the database");
                return allRecords;
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

    private async Task SaveAllRecordsAsync(List<Airbnb> records)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Remove duplicates within the uploaded data, based on AirbnbId
                    var uniqueRecords = records
                        .GroupBy(f => f.AirbnbId)
                        .Select(g => g.First())
                        .ToList();

                    // Get existing AirbnbIds from the database
                    var incomingAirbnbIds = uniqueRecords.Select(f => f.AirbnbId).ToList();
                    var existingAirbnbs = await context.Airbnbs
                        .Where(f => incomingAirbnbIds.Contains(f.AirbnbId))
                        .Select(f => f.AirbnbId)
                        .ToListAsync();

                    // Filter only new records that don't exist in the database
                    var newRecords = uniqueRecords
                        .Where(f => !existingAirbnbs.Contains(f.AirbnbId))
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
                    _logger.LogError(ex, "Error saving records to database");
                    await transaction.RollbackAsync();
                    throw new ApplicationException("Error saving records to database", ex);
                }
            }
        }
    }
}



    // private async Task SaveAllRecordsAsync(List<Airbnb> records)
    // {
    //     using (var context = _contextFactory.CreateDbContext())
    //     {
    //         using (var transaction = await context.Database.BeginTransactionAsync())
    //         {
    //             try
    //             {
    //                 var uniqueRecords = records.GroupBy(f => f.AirbnbId).Select(g => g.First()).ToList();
    //                 var recordIds = uniqueRecords.Select(f => f.Id).ToList();
    //                 var existingAirbnbs = await context.Airbnbs.Where(f => recordIds.Contains(f.AirbnbId)).ToListAsync();

    //                 var newRecords = uniqueRecords
    //                     .Where(f => !existingAirbnbs.Any(existing => existing.Id == f.Id))
    //                     .ToList();

    //                 if (newRecords.Any())
    //                 {
    //                     await context.Airbnbs.AddRangeAsync(newRecords);
    //                     await context.SaveChangesAsync();
    //                 }

    //                 await transaction.CommitAsync();
    //                 _logger.LogInformation($"Saved {newRecords.Count} records to database");
    //             }
    //             catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
    //             {
    //                 _logger.LogError(ex, "Duplicate key value violates unique constraint 'PK_Airbnbs'");
    //                 await transaction.RollbackAsync();
    //             }
    //             catch (Exception ex)
    //             {
    //                 _logger.LogError(ex, "Error saving records to database");
    //                 await transaction.RollbackAsync();
    //                 throw new ApplicationException("Error saving records to database", ex);
    //             }
    //         }
    //     }
    // }