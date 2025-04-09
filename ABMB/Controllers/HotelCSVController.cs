using ABMB.Models;
using ABMB.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using ABMB.Hotels;

[ApiController]
[Route("hotels/")]
public class HotelCSVController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public HotelCSVController(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [HttpPost("upload")]
    public IActionResult ImportFromLocalFile()
    {
        HotelCSVUploader hotelUploader = new(_contextFactory);
        hotelUploader.InsertCSVUsingThreadPool();

        return Ok(new { Message = "hotels inserted from local file." });
    }
}
