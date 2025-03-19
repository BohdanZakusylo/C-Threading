using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using ABMB.Models;

namespace ABMB.Controllers
{
    public class CsvFileReader
    {
        private readonly string _filePath;

        public CsvFileReader(string filePath)
        {
            _filePath = filePath;
        }

        public IEnumerable<OldFlight> ReadCsvFile()
        {
            using var reader = new StreamReader(_filePath);
            using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<OldFlight>();
            return records;
        }
    }
}