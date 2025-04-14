using System.Globalization;
using ABMB.Models;
using ABMB.Properties;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public class AirbnbService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    private static readonly object _saveLock = new();

    public AirbnbService(
        IDbContextFactory<AppDbContext> contextFactory
    )
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<Airbnb>> ReadCsvFileAirbnb(Stream fileStream)
    {
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture);

            using (var reader = new StreamReader(fileStream))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<AirbnbMap>();

                var allRecords = new List<Airbnb>();
                await foreach (var record in csv.GetRecordsAsync<Airbnb>())
                {
                    allRecords.Add(record);
                }

                lock (_saveLock)
                {
                    SaveAllRecordsAsync(allRecords).GetAwaiter().GetResult();
                }

                return allRecords;
            }
        }
        catch (HeaderValidationException e)
        {
            throw new ApplicationException("CSV file header is invalid.", e);
        }
        catch (TypeConverterException ex)
        {
            throw new ApplicationException("CSV file contains invalid data format.", ex);
        }
        catch (Exception e)
        {
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
                    var uniqueRecords = records
                        .GroupBy(f => f.AirbnbId)
                        .Select(g => g.First())
                        .ToList();

                    var incomingAirbnbIds = uniqueRecords.Select(f => f.AirbnbId).ToList();
                    var existingAirbnbs = await context
                        .Airbnbs.Where(f => incomingAirbnbIds.Contains(f.AirbnbId))
                        .Select(f => f.AirbnbId)
                        .ToListAsync();

                    var newRecords = uniqueRecords
                        .Where(f => !existingAirbnbs.Contains(f.AirbnbId))
                        .ToList();

                    if (newRecords.Any())
                    {
                        await context.Airbnbs.AddRangeAsync(newRecords);
                        await context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                    when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new ApplicationException("Error saving records to database", ex);
                }
            }
        }
    }
}
