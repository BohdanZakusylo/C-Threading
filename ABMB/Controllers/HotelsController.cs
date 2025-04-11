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
    public async Task<string> Get()
    {
        HotelDataOperator hotelDataOperator = new("Merlo", _context, "2025-11-12", "2025-11-15");
        await hotelDataOperator.GetValidHotelIds();
        return "Hello";
    }
}
