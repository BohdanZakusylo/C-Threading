using System.Globalization;
using ABMB.Models;
using ABMB.Properties;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

// Include the namespace where the map resides

public class CsvService
{
    private readonly AppDbContext _context;

    public CsvService(AppDbContext context)
    {
        _context = context;
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
                await foreach (var record in csv.GetRecordsAsync<OldFlight>()) records.Add(record);
                foreach (var record in records)
                {
                    var existingRecord = await _context.OldFlights.FindAsync(record.Id);
                    if (existingRecord == null)
                    {
                        await _context.OldFlights.AddAsync(record);
                    }
                }
                await  _context.OldFlights.AddRangeAsync(records);
                await _context.SaveChangesAsync();
                
                Console.WriteLine("ok");
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
}