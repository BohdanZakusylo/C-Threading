using System.Threading.Tasks;
using ABMB.Hotels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace ABMB.Controllers;

[ApiController]
[Route("/get/hotels")]
public class HotelsController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching",
    };

    private readonly ILogger<HotelsController> _logger;

    public HotelsController(ILogger<HotelsController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetHotels")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        HotelDestinationRetriever hdr = new();
        HotelRetriever htr = new();

        string hotelid = await hdr.RetreiveDestination("Kyiv");

        if (hotelid == null)
        {
            var hotelIds = await htr.RetrieveHotelIds(hotelid);
            Console.WriteLine(hotelIds);
        }

        return Enumerable
            .Range(1, 5)
            .Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)],
            })
            .ToArray();
    }
}
