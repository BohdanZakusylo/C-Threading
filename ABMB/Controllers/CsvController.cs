using Microsoft.AspNetCore.Mvc;

namespace ABMB.Controllers.CSV;

[ApiController]
[Route("post/flights")]
public class CsvController : ControllerBase
{
    private readonly CsvService _csvService;

    public CsvController(CsvService csvService)
    {
        _csvService = csvService;
    }

    [HttpPost]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> Post(IFormFile csvFile)
    {
        if (csvFile != null && csvFile.Length > 0)
        {
            await using var stream = csvFile.OpenReadStream();
            try
            {
                var records = (await _csvService.ReadCsvFile(stream)).ToList();
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
                return StatusCode(
                    500,
                    new { message = "Internal error occurred.", details = e.Message }
                );
            }
        }

        return BadRequest(new { message = "Please select a valid CSV file." });
    }
}
