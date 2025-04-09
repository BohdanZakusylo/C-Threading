using ABMB.Models;
using ABMB.Properties;
using CsvHelper;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace ABMB.Hotels
{
    public class HotelCSVUploader
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public HotelCSVUploader(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void InsertCSVUsingThreadPool()
        {
            var filePath = "./Data/new_hotels.csv";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("CSV file not found.");
                return;
            }

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<HotelMap>();

            var hotels = csv.GetRecords<Hotel>().ToList();

            int batchSize = 500;
            int maxConcurrency = 30;

            var semaphore = new SemaphoreSlim(maxConcurrency);
            var countdown = new CountdownEvent((int)Math.Ceiling(hotels.Count / (double)batchSize));

            for (int i = 0; i < hotels.Count; i += batchSize)
            {
                var batch = hotels.Skip(i).Take(batchSize).ToList();

                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        using var context = _contextFactory.CreateDbContext();
                        context.Hotels.AddRange(batch);
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        semaphore.Release();
                        countdown.Signal();
                    }
                });
            }
            countdown.Wait();
        }
    }
}
