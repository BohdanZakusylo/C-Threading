using System.ComponentModel.DataAnnotations;
using ABMB.Models;
using ABMB.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABMB.Controllers;

[ApiController]
[Route("get/oldflightprice")]
public class OldFlightPriceController : ControllerBase
{
    private readonly AppDbContext _appContext;

    public OldFlightPriceController(AppDbContext appContext)
    {
        _appContext = appContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetPrice([FromQuery] FlightRequest flightRequest)
    {
        if (flightRequest == null)
        {
            return BadRequest(new { message = "Invalid request. FlightRequest cannot be null." });
        }

        var departureId = flightRequest.DepartureId;
        var arrivalId = flightRequest.ArrivalId;

        if (string.IsNullOrEmpty(departureId) || string.IsNullOrEmpty(arrivalId))
        {
            return BadRequest(new { message = "DepartureId and ArrivalId are required." });
        }

        var flights = await _appContext
            .OldFlights.AsNoTracking()
            .Where(f => f.Origin == departureId && f.Destination == arrivalId)
            .ToListAsync();

        if (!flights.Any())
        {
            return NotFound(new { message = "No flights found for the given criteria." });
        }

        return Ok(flights);
    }

    public class FlightRequest
    {
        [Required]
        public string DepartureId { get; set; }

        [Required]
        public string ArrivalId { get; set; }
    }
}
