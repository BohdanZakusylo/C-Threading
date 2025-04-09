using Microsoft.AspNetCore.Mvc;

namespace ABMB.Controllers;

[ApiController]
[Route("api/hello")]
public class HelloController : ControllerBase
{
    private readonly CsvService _csvService;

    public HelloController(CsvService csvService)
    {
        _csvService = csvService;
    }

    [HttpPost]
    public async Task<IActionResult> Post(IFormFile csvFile)
    {
        if (csvFile != null && csvFile.Length > 0)
            return Ok("Hello World");

        return Ok("not World");
    }
}
