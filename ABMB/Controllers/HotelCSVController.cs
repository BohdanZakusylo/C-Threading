using ABMB.Models;
using ABMB.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using ABMB.Hotels;

[ApiController]
[Route("post/")]
public class HotelCSVController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public HotelCSVController(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [HttpPost("hotels")]
    [RequestSizeLimit(100_000_000)]
    public IActionResult ImportFromLocalFile(IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
            return BadRequest(new { Message = "No CSV file uploaded." });

        try
        {
            using var stream = csvFile.OpenReadStream();

            HotelCSVUploader hotelUploader = new(_contextFactory);
            hotelUploader.InsertCSVUsingThreadPool(stream);

            return Ok(new { Message = "hotels inserted from local file." });
        }
        catch
        {
            return BadRequest("An error occured with the file");
        }
    }
}
