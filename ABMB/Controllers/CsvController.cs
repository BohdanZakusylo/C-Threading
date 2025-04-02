using Microsoft.AspNetCore.Mvc;

namespace ABMB.Controllers.CSV;

[ApiController]
[Route("api/upload")]
public class CsvController : ControllerBase
{
    private readonly CsvService _csvService;

    public CsvController(CsvService csvService)
    {
        _csvService = csvService;
    }


    [HttpPost]
    public async Task<IActionResult> Post(IFormFile csvFile)
    {
        if (csvFile != null && csvFile.Length > 0)
        {
            await using var stream = csvFile.OpenReadStream();
            try
            {
                var records = (await _csvService.ReadAirbnbCsvFile(stream)).ToList();
                return Ok(new { message = "Fields added successfully", data = records });
            }
            catch (ApplicationException e)
            {
                Console.WriteLine(e);
                return BadRequest(new { message = e.Message });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500, new { message = "Internal error occurred.", details = e.Message });
            }
        }

        return BadRequest(new { message = "Please select a valid CSV file." });
    }
}

// using ABMB.Properties;
// using Microsoft.AspNetCore.Mvc;

// namespace ABMB.Controllers.CSV;
// [ApiController]
// [Route("api/addCsv")]
// public class CsvController : ControllerBase
// {
    
//     private readonly CsvService _csvService;
//     private readonly AppDbContext _appDbContext;
    
//     public CsvController(CsvService csvService, AppDbContext appDbContext)
//     {
//         _csvService = csvService;
//         _appDbContext = appDbContext;
//     }

//     [HttpPost]
//     public async Task<IActionResult> PopolcateDatabaseFromCsv()
//     {
//       try
//         {
//             var flightRecords = await _csvService.ReadFlightCsvFileAsync();
//             var airbnbRecords = await _csvService.ReadAirbnbCsvFileAsync();

//             if ((flightRecords == null || !flightRecords.Any()) && (airbnbRecords == null || !airbnbRecords.Any()))
//             {
//                 return BadRequest(new { message = "CSV files contain no valid records." });
//             }

//             if (flightRecords?.Any() == true)
//             {
//                 await _appDbContext.AddRangeAsync(flightRecords);
//             }

//             if (airbnbRecords?.Any() == true)
//             {
//                 await _appDbContext.AddRangeAsync(airbnbRecords);
//             }

//             await _appDbContext.SaveChangesAsync();

//             return Ok(new { message = "Database populated successfully." });
//         }
//         catch (Exception e)
//         {
//             return BadRequest(new { message = "An error occurred while processing the CSV file.", error = e.Message });
//         }
//     }
    
// }