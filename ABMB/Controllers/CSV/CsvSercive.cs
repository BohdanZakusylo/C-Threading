using System.Collections.Concurrent;
using System.Globalization;
using ABMB.Models;
using ABMB.Properties;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.EntityFrameworkCore;
using Npgsql;

// Include the namespace where the map resides

public class CsvService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CsvService> _logger;
    private const int BatchSize = 1000;

    public CsvService(IDbContextFactory<AppDbContext> contextFactory, ILogger<CsvService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;

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

                var records = new List<OldFlight>();
                var batch = new List<OldFlight>();

                await foreach (var record in csv.GetRecordsAsync<OldFlight>())
                {
                    records.Add(record);
                    batch.Add(record);

                    if (batch.Count >= BatchSize)
                    {
                        await SaveBatchAsync(batch);
                        batch = new List<OldFlight>();
                    }
                }

                if (batch.Count > 0)
                {
                    await SaveBatchAsync(batch);
                }

                return records;
            }
        }
        catch (HeaderValidationException e)
        {
            Console.WriteLine(e);
            throw new ApplicationException("CSV file header is invalid.", e);
        }
        catch (TypeConverterException ex)
        {
            Console.WriteLine(ex);
            throw new ApplicationException("CSV file contains invalid data format.", ex);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new ApplicationException("Error reading CSV file", e);
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

                    foreach (var record in uniqueBatch)
                    {
                        var existingRecord = await context.OldFlights
                            .FirstOrDefaultAsync(f => f.Id == record.Id);

                        if (existingRecord == null)
                        {
                            await context.OldFlights.AddAsync(record);
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
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