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
            var records = await _csvService.ReadCsvFileAsync();
            await _appDbContext.AddRangeAsync(records);
            await _appDbContext.SaveChangesAsync();
            return Ok(new { message = "Database populated" });
        }
        catch (Exception e)
        {
            return BadRequest(new { message = "An error occurred while processing the CSV file.", error = e.Message });
        }
    }
    
}