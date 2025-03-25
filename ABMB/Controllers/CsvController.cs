using ABMB.Properties;
using Microsoft.AspNetCore.Mvc;

namespace ABMB.Controllers.CSV;
[ApiController]
[Route("api/addCsv")]
public class CsvController : ControllerBase
{
    
    private readonly CsvService _csvService;
    private readonly AppDbContext _appDbContext;
    
    public CsvController(CsvService csvService, AppDbContext appDbContext)
    {
        _csvService = csvService;
        _appDbContext = appDbContext;
    }

    [HttpPost]
    public async Task<IActionResult> PopolcateDatabaseFromCsv()
    {
      try
        {
            var flightRecords = await _csvService.ReadFlightCsvFileAsync();
            var airbnbRecords = await _csvService.ReadAirbnbCsvFileAsync();

            if ((flightRecords == null || !flightRecords.Any()) && (airbnbRecords == null || !airbnbRecords.Any()))
            {
                return BadRequest(new { message = "CSV files contain no valid records." });
            }

            if (flightRecords?.Any() == true)
            {
                await _appDbContext.AddRangeAsync(flightRecords);
            }

            if (airbnbRecords?.Any() == true)
            {
                await _appDbContext.AddRangeAsync(airbnbRecords);
            }

            await _appDbContext.SaveChangesAsync();

            return Ok(new { message = "Database populated successfully." });
        }
        catch (Exception e)
        {
            return BadRequest(new { message = "An error occurred while processing the CSV file.", error = e.Message });
        }
    }
    
}