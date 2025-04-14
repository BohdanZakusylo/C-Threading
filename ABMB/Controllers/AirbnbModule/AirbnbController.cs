using ABMB.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABMB.Controllers.AirbnbModule;

[ApiController]
[Route("get")]
public class AirbnbController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly AirBnBPriceRetriever _airBnBPriceRetriever;
    private readonly AirbnbListingsRetriever _airbnbListingsRetriever;
    private readonly string RapidApiKey = Environment.GetEnvironmentVariable("RAPID_API_KEY")!;
    private const string RapidApiHost = "airbnb-listings.p.rapidapi.com";

    public AirbnbController(AppDbContext context)
    {
        _context = context;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", RapidApiKey);
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", RapidApiHost);
        _airBnBPriceRetriever = new AirBnBPriceRetriever();
        _airbnbListingsRetriever = new AirbnbListingsRetriever(context);
    }

    [HttpGet("airbnb")]
    public async Task<IActionResult> GetAirbnbs(
        [FromQuery] string country,
        [FromQuery] string city,
        [FromQuery] string date
    )
    {
        try
        {
            if (string.IsNullOrEmpty(country))
            {
                return BadRequest("Country parameter is required");
            }

            if (string.IsNullOrEmpty(city))
            {
                return BadRequest("City parameter is required");
            }

            if (string.IsNullOrEmpty(date))
            {
                return BadRequest("Date parameter is required");
            }

            var month = date.Substring(5, 2);
            var year = date.Substring(0, 4);

            var matchingIds = await SearchListings(country, city);

            if (!matchingIds.Any())
                return NotFound("No listings found matching the criteria");

            List<String> matchingIdsAsString = new List<String>();

            foreach (var id in matchingIds)
            {
                matchingIdsAsString.Add(id.ToString());

                if (matchingIdsAsString.Count >= 10)
                {
                    break;
                }
            }

            var availableListings =
                await _airbnbListingsRetriever.CheckMultipleListingsAvailability(
                    matchingIdsAsString,
                    int.Parse(year),
                    month,
                    date
                );

            var result = new List<Object>();

            foreach (var id in availableListings)
            {
                var data = await _airbnbListingsRetriever.RetrieveDataForListings(long.Parse(id));

                var currentPrice = await _airBnBPriceRetriever.GetListingPrice(
                    id,
                    int.Parse(year),
                    month
                );

                var priceComparison = await _airBnBPriceRetriever.GetPriceComparison(
                    id,
                    int.Parse(year),
                    month
                );

                if (currentPrice != 0)
                {
                    var listingData = data.FirstOrDefault() as IDictionary<string, object>;

                    if (listingData != null)
                    {
                        listingData["currentPrice"] = currentPrice;
                        listingData["priceComparison"] = priceComparison;
                        result.Add(listingData);
                    }
                }
            }
            Console.WriteLine(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    public async Task<List<long>> SearchListings(string country, string city)
    {
        try
        {
            var resultList = new List<long>();
            var foundListings = await _context
                .Airbnbs.Where(a => a.Country.ToLower().Trim() == country.ToLower().Trim())
                .ToListAsync();

            if (foundListings.Count == 0)
            {
                return resultList;
            }

            List<long> matchingIds = foundListings
                .Where(a =>
                    a.Address != null && a.Address.ToLower().Contains(city.ToLower().Trim())
                )
                .Select(a => a.AirbnbId)
                .ToList();

            if (matchingIds.Count == 0)
            {
                return resultList;
            }

            resultList = matchingIds;

            return resultList;
        }
        catch (Exception ex)
        {
            throw new Exception("No listings found matching the criteria");
        }
    }
}
