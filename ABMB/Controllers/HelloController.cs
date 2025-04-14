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

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Hello from ABMB API!" });
    }
}
