using System.Threading.Tasks;
using ABMB.Hotels;
using ABMB.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace ABMB.Controllers;

[ApiController]
[Route("/get/")]
public class HotelsController : ControllerBase
{
    private readonly AppDbContext _context;

    public HotelsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("hotels")]
    public async Task<IActionResult> Get(string destination, string arrivalDate, string departureDate)
    {
        // HotelDataOperator hotelDataOperator = new("Merlo", _context, "2025-11-12", "2025-11-15");
        HotelDataOperator hotelDataOperator = new(destination, _context, arrivalDate, departureDate);
        List<HotelModel> hotels = await hotelDataOperator.GetValidHotelIds();

        var result = hotels.Select(h => new HotelReturnModel
        {
            price = h.ApiHotelModel.Price,
            currency = h.ApiHotelModel.currency,
            url = h.ApiHotelModel.Url,
            availableRooms = h.ApiHotelModel.available_rooms,
            countryName = h.dbHotel.countyName,
            hotelName = h.dbHotel.HotelName,
            phoneNumber = h.dbHotel.PhoneNumber
        });

        return Ok(result);
    }
}
