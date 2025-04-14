using Microsoft.AspNetCore.Mvc;
using ABMB.Models;
using System.Dynamic;
using ABMB.Properties;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using ABMB.Controllers.AirbnbModule;


namespace ABMB.Controllers.AirbnbModule;

[ApiController]
[Route("api/airbnb")]
public class AirbnbController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AirbnbController> _logger;
    private readonly HttpClient _httpClient;
    private readonly AirbnbDbRetriever _airbnbDbRetriever;
    private readonly AirBnBPriceRetriever _airBnBPriceRetriever;
    private readonly AirbnbListingsRetriever _airbnbListingsRetriever;

    private readonly AirbnbUtils _airbnbUtils;
    private const string RapidApiKey = "d4d0ef677fmsha3418e97ed26df9p12df8bjsn6c2d64ebd551";
    private const string RapidApiHost = "airbnb-listings.p.rapidapi.com";

    public AirbnbController(AppDbContext context, ILogger<AirbnbController> logger, IHttpClientFactory httpClientFactory, AirBnBPriceRetriever airBnBPriceRetriever, AirbnbListingsRetriever airbnbListingsRetriever)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", RapidApiKey);
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", RapidApiHost);
        _airbnbDbRetriever = new AirbnbDbRetriever(context);
        _airbnbUtils = new AirbnbUtils();
        _airBnBPriceRetriever = airBnBPriceRetriever;
        _airbnbListingsRetriever = airbnbListingsRetriever;
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetAirbnbs([FromQuery] string country, [FromQuery] string city)
    {
        try
        {
            if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(city))
                return BadRequest("Country and city parameters are required");

            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var month = DateTime.Now.ToString("MM");
            var year = DateTime.Now.ToString("yyyy");

            var matchingIds = await SearchListings(country, city);

            if (!matchingIds.Any())
                return NotFound("No listings found matching the criteria");

            _logger.LogInformation($"Found {matchingIds.Count} listings in {country} with keyword: {city}");

            List<String> matchingIdsAsString = new List<String>();

            foreach (var id in matchingIds){
                matchingIdsAsString.Add(id.ToString());

                if(matchingIdsAsString.Count >= 10){
                    break;
                }
            }

            var availableListings = await _airbnbListingsRetriever.CheckMultipleListingsAvailability(
                matchingIdsAsString,
                int.Parse(year),
                month,
                date
            );

            var result = new List<Object>();

            foreach(var id in availableListings)
            {
                var data = await _airbnbListingsRetriever.RetrieveDataForListings(long.Parse(id));
                _logger.LogInformation($"Data for listing {id}: {data}");

                var currentPrice = await _airBnBPriceRetriever.GetListingPrice(id, int.Parse(year), month);
                _logger.LogInformation($"Current price for listing {id}: {currentPrice}");

                var priceComparison = await _airBnBPriceRetriever.GetPriceComparison(id, int.Parse(year), month);
                _logger.LogInformation($"Price comparison for listing {id}: {priceComparison}");

                if(currentPrice != 0){
                    
                    var listingData = data.FirstOrDefault() as IDictionary<string, object>;

                    if (listingData != null)
                    {
                        listingData["currentPrice"] = currentPrice;
                        listingData["priceComparison"] = priceComparison;
                        result.Add(listingData);
                    }

                    _logger.LogInformation($"Listing data: {listingData}");

                }
            }

            return Ok(result);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAirbnbs");
            return StatusCode(500, "An internal server error occurred.");
        }
    }


    public async Task<List<long>> SearchListings(string country, string city)
    {
        try
        {
          
            _logger.LogInformation($"Searching for listings in {country} with keyword: {city}");

            var resultList = new List<long>();
            var foundListings = await _context.Airbnbs
                .Where(a => a.Country.ToLower().Trim() == country.ToLower().Trim())
                .ToListAsync();


            _logger.LogInformation($"Found {foundListings.Count} listings in {country}");

            _logger.LogInformation($"Found {foundListings.ToString} listings in {country}");

            if(foundListings.Count == 0){
                return resultList;
            }


            List<long> matchingIds = foundListings
                .Where(a => a.Address != null && a.Address.ToLower().Contains(city.ToLower().Trim()))
                .Select(a => a.AirbnbId)
                .ToList();

            _logger.LogInformation($"Found {matchingIds.Count} matching listings");

            if (matchingIds.Count == 0)
            {
                return resultList;
            }

            resultList = matchingIds;

            return resultList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching listings");
            throw new Exception("No listings found matching the criteria");
        }
    }
}
