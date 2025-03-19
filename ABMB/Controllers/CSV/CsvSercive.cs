using System.Collections.Generic;
using System.Threading.Tasks;
using ABMB.Models;
using Microsoft.Extensions.Configuration;

namespace ABMB.Controllers
{
    public class CsvService
    {
        private readonly CsvFileReader _csvReader;

        public CsvService(IConfiguration configuration)
        {
            var filePath = configuration.GetValue<string>("CsvSettings:FilePath"); // Read from appsettings.json
            _csvReader = new CsvFileReader(filePath);
        }

        public async Task<IEnumerable<OldFlight>> ReadCsvFileAsync()
        {
            return await Task.Run(() => _csvReader.ReadCsvFile());
        }
    }
}