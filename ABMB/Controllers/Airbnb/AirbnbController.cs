using Microsoft.AspNetCore.Mvc;

namespace ABMB.Controllers.Airbnb;

[ApiController]
[Route("post/upload/airbnb")]
public class AirbnbController : ControllerBase
{
    private readonly AirbnbService _airbnbService;
    
    public AirbnbController(AirbnbService airbnbService)
    {
        _airbnbService = airbnbService;
    }

    [HttpPost]
    public async Task<IActionResult> Post(IFormFile csvFile)
    {
        if (csvFile != null && csvFile.Length > 0)
        {
            await using var stream = csvFile.OpenReadStream();
            try
            {
                var records = (await _airbnbService.ReadCsvFileAirbnb(stream)).ToList();
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
                throw;
            }
        }

        return BadRequest(new { message = "Please select a valid CSV file." });
    }
}