using System.ComponentModel.DataAnnotations;
using ABMB.Models;
using ABMB.Properties;
using Microsoft.AspNetCore.Mvc;

namespace ABMB.Controllers;
[ApiController]
[Route("api/oldflightprice")]
public class OldFlightPriceController : ControllerBase
{
    private readonly AppDbContext _appContext;
    
    public OldFlightPriceController(AppDbContext appContext)
    {
        _appContext = appContext;
    }

    [HttpGet]
    public Task<IEnumerable<OldFlight>> GetPrice([FromBody] FlightRequest flightRequest)
    {
        if (flightRequest == null)
        {
            return Task.FromResult<IEnumerable<OldFlight>>(new List<OldFlight>());
        }

        var departureId = flightRequest.DepartureId;
        var arrivalId = flightRequest.ArrivalId;

        if (string.IsNullOrEmpty(departureId) || string.IsNullOrEmpty(arrivalId))
        {
            return Task.FromResult<IEnumerable<OldFlight>>(new List<OldFlight>());
        }
        var flights = _appContext.OldFlights
            .Where(f => f.Origin == departureId && f.Destination == arrivalId)
            .ToList();
        if (flights.Count == 0)
        {
            return Task.FromResult<IEnumerable<OldFlight>>(new List<OldFlight>());
        }

        return Task.FromResult<IEnumerable<OldFlight>>(flights);
    }



    public class FlightRequest
    {
        [Required]
        public string DepartureId { get; set; }
        [Required]
        public string ArrivalId { get; set; }
    }

}