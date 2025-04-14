using Microsoft.AspNetCore.Mvc;

namespace ABMB.Controllers.AirbnbModule;

[ApiController]
[Route("post/airbnb")]
public class AirbnbCsvController : ControllerBase
{
    private readonly AirbnbService _airbnbService;

    
    public AirbnbCsvController(AirbnbService airbnbService)
    {
        _airbnbService = airbnbService;
    }

    [HttpPost]
    public async Task<IActionResult> Post(IFormFile csvFile)
    {
        if (csvFile == null)
        {
            Console.WriteLine("Error: No file was uploaded.");
            return BadRequest(new { message = "No file was uploaded." });
        }

        if (csvFile.Length > 50 * 1024 * 1024)
        {
            Console.WriteLine($"Error: File size {csvFile.Length} bytes exceeds the 50MB limit.");
            return BadRequest(new { message = "File size exceeds the 50MB limit." });
        }

        var extension = Path.GetExtension(csvFile.FileName).ToLowerInvariant();
        if (extension != ".csv")
        {
            Console.WriteLine($"Error: Invalid file extension {extension}. Only CSV files are allowed.");
            return BadRequest(new { message = "Only CSV files are allowed." });
        }

        try
        {
            await using var stream = csvFile.OpenReadStream();
            var records = (await _airbnbService.ReadCsvFileAirbnb(stream)).ToList();
            
            if (!records.Any())
            {
                Console.WriteLine("Warning: File processed but contains no records.");
                return NoContent();
            }

            Console.WriteLine($"Success: Processed {records.Count} records from file {csvFile.FileName}");
            return Ok(new { 
                message = "File processed successfully", 
                recordsProcessed = records.Count,
                data = records 
            });
        }
        catch (ApplicationException e)
        {
            Console.WriteLine($"Application Error: {e.Message}");
            return BadRequest(new { 
                message = "Error processing file", 
                error = e.Message 
            });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected Error: {e.Message}");
            return StatusCode(500, new { 
                message = "An unexpected error occurred", 
                error = e.Message 
            });
        }
    }


}