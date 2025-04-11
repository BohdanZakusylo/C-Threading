using Microsoft.AspNetCore.Mvc;

namespace ABMB.Controllers.CSV;

[ApiController]
[Route("api/upload/airbnb")]
public class AirbnbCsvController : ControllerBase
{
    private readonly AirbnbCsvService _csvService;

    public AirbnbCsvController(AirbnbCsvService csvService)
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
