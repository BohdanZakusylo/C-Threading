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
        TestHandler handler = new();
        HotelDestinationRetriever htdr = new();
        HotelDataRetriever hdr = new();

        foreach (var hotelData in handler.HotelsModelList)
        {
            string? hotel_id = await htdr.RetreiveDestination(
                hotelData.CountryName!,
                hotelData.HotelName!
            );

            if (hotel_id != null)
            {
                await hdr.RetreiveHotelInfo(hotel_id);
            }
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
