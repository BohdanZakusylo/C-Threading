using Microsoft.AspNetCore.Mvc;
using ABMB.Models;
using System.Dynamic;

using ABMB.Properties;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ABMB.Controllers.AirbnbModule;

[ApiController]
[Route("api/airbnb")]
public class AirbnbController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AirbnbController> _logger;
    private readonly HttpClient _httpClient;
    private readonly AirbnbDbRetriever _airbnbDbRetriever;
    private const string RapidApiKey = "5b010fa62cmsh255e51d4d0d21d5p199a6bjsnb86beb97bb68";
    private const string RapidApiHost = "airbnb-listings.p.rapidapi.com";

    public AirbnbController(AppDbContext context, ILogger<AirbnbController> logger, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", RapidApiKey);
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", RapidApiHost);
        _airbnbDbRetriever = new AirbnbDbRetriever(context);
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

            var availableListings = await CheckMultipleListingsAvailability(
                matchingIdsAsString,
                int.Parse(year),
                month,
                date
            );

            var result = new List<Object>();

            foreach(var id in availableListings)
            {
                var data = await RetrieveDataForListings(long.Parse(id));
                _logger.LogInformation($"Data for listing {id}: {data}");

                var currentPrice = await GetListingPrice(id, int.Parse(year), month);
                _logger.LogInformation($"Current price for listing {id}: {currentPrice}");

                var priceComparison = await GetPriceComparison(id, int.Parse(year), month);
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

  // [HttpGet("price-comparison")]
    public async Task<decimal> GetPriceComparison(
        string airbnbId,
        int year,
        string month)
    {
        try
        {
            // Get current month's price
            var currentPrice = await GetListingPrice(airbnbId, year, month);
            if (currentPrice == 0)
            {
                _logger.LogWarning($"Current price is 0 for listing comparison. airbnbId: {airbnbId}");
            }

            // Calculate previous month
            var (previousYear, previousMonth) = GetPreviousMonth(year, int.Parse(month));

            // Get previous month's price
            var previousPrice = await GetListingPrice(airbnbId, previousYear, previousMonth.ToString("D2"));
            // var previousPrice = await GetPriceAsync(id, previousYear, previousMonth.ToString("D2"), "previous");
            if (previousPrice == 0)
            {
                _logger.LogWarning($"Previous price is 0 for listing comparison. currentPrice: {currentPrice}");
            }

            _logger.LogWarning($"Previous price is 0 for listing comparison. currentPrice: {currentPrice}");

            decimal percentageDifference = CalculatePercentageDifference(currentPrice, previousPrice);
            _logger.LogInformation($"Percentage difference: {percentageDifference}");

            return percentageDifference;    
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error comparing prices for listing {airbnbId}");
            throw new Exception($"An error occurred while comparing prices, {ex.Message}");
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

public async Task<List<ExpandoObject>> RetrieveDataForListings(long airbnbId)
{
    try
    {
        _logger.LogInformation($"Searching for listings in {airbnbId}");

        var foundListings = await _context.Airbnbs
            .Where(a => a.AirbnbId == airbnbId)
            .Select(a => new
            {
                a.AirbnbId,
                a.Name,
                a.Address,
                a.ImgLinks,
                a.Country,
                a.Features,
                a.Toilets,
                a.Studios,
                a.CheckIn,
                a.CheckOut
            })
            .ToListAsync();

        _logger.LogInformation($"Found {foundListings.Count} listings in {airbnbId}");

        if (!foundListings.Any())
        {
            return new List<ExpandoObject>();
        }

        var result = new List<ExpandoObject>();

        foreach (var listing in foundListings)
        {
            dynamic expando = new ExpandoObject();
            var dict = (IDictionary<string, object>)expando;

            dict["airbnbId"] = listing.AirbnbId;
            dict["name"] = listing.Name;
            dict["address"] = listing.Address;
            dict["imgLinks"] = listing.ImgLinks;
            dict["country"] = listing.Country;
            dict["features"] = listing.Features;
            dict["toilets"] = listing.Toilets;
            dict["studios"] = listing.Studios;
            dict["checkIn"] = listing.CheckIn;
            dict["checkOut"] = listing.CheckOut;

            result.Add(expando);
        }

        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error searching listings");
        throw new Exception("No listings found matching the criteria");
    }
}



// public async Task<List<object>> RetrieveDataForListings(long airbnbId)
// {
//     try
//     {
//         _logger.LogInformation($"Searching for listings in {airbnbId}");

//         var foundListings = await _context.Airbnbs
//             .Where(a => a.AirbnbId == airbnbId)
//             .Select(a => new
//             {
//                 a.AirbnbId,
//                 a.Name,
//                 a.Address,
//                 a.ImgLinks,
//                 a.Country,
//                 a.Features,
//                 a.Toilets,
//                 a.Studios,
//                 a.CheckIn,
//                 a.CheckOut
//             })
//             .ToListAsync();

//         _logger.LogInformation($"Found {foundListings.Count} listings in {airbnbId}");

//         if (foundListings.Count == 0)
//         {
//             return new List<object>();
//         }

//         var resultList = foundListings.Select(a => new
//         {
//             a.AirbnbId,
//             a.Name,
//             a.Address,
//             a.ImgLinks,
//             a.Country,
//             a.Features,
//             a.Toilets,
//             a.Studios,
//             a.CheckIn,
//             a.CheckOut
//         }).Cast<object>().ToList();

//         return resultList;
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Error searching listings");
//         throw new Exception("No listings found matching the criteria");
//     }
// }


public async Task<List<string>> CheckMultipleListingsAvailability(List<string> ids, int year, string month, string date)
{
    var availableIds = new List<string>();

    try
    {
        _logger.LogInformation($"Checking availability for {ids.Count} listings");

        foreach (var id in ids)
        {
            try
            {
                bool isAvailable = await CheckAvailabilityForId(id, year, month, date);

                if (isAvailable)
                {
                    availableIds.Add(id);

                    // Stop if we've found 5 available listings
                    if (availableIds.Count >= 5)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking availability for ID {id}");
                continue;
            }
        }

        _logger.LogInformation($"Found {availableIds.Count} available out of {ids.Count}");
        return availableIds;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error checking multiple listings availability");
        throw;
    }
}

public async Task<bool> CheckAvailabilityForId(string airbnbId, int year, string month, string date)
{
    try
    {
        _logger.LogInformation($"Checking availability for listing airbnbId:{airbnbId} in month:{month} year:{year} and date:{date}");

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://{RapidApiHost}/v2/listingavailability?id={airbnbId}&year={year}&month={month}"),
            Headers =
            {
                { "X-RapidAPI-Key", RapidApiKey },
                { "X-RapidAPI-Host", RapidApiHost }
            }
        };

        using var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning($"Failed API call for listing {airbnbId}, response: {content}");
            return false;
        }

        var availabilityData = JsonSerializer.Deserialize<AvailabilityResponse>(content);
        if (availabilityData?.Results == null)
        {
            _logger.LogWarning($"No 'results' in API response for listing {airbnbId}. Raw: {content}");
            return false;
        }

        var availability = availabilityData.Results.FirstOrDefault(r => r.Date == date);
        if (availability == null)
        {
            _logger.LogWarning($"No entry for date {date} in availability results for {airbnbId}");
            return false;
        }

        return availability.Available == 1;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error checking availability for listing {airbnbId}");
        return false;
    }
}

    public async Task<decimal> GetListingPrice(string airbnbId, int year, string month)
    {
        try
        {
            _logger.LogInformation($"Getting price for listing {airbnbId} in {month} {year}");

             var request = new HttpRequestMessage{
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://{RapidApiHost}/v2/listingPrices?id={airbnbId}&year={year}&month={month}"),
                Headers = {
                    { "X-RapidAPI-Key", RapidApiKey },
                    { "X-RapidAPI-Host", RapidApiHost }
                }
            };

            using (var response = await _httpClient.SendAsync(request))
            {

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"RapidAPI request failed with status code: {response.StatusCode}");
                    throw new Exception($"Failed to get listing price, {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw response: {Response}", content);
                var priceData = JsonSerializer.Deserialize<PriceResponse>(content);

                if (priceData?.Results == null || !priceData.Results.Any())
                {
                    throw new Exception("No price data available for the specified period");
                }

                // Calculate average price
                var averagePrice = priceData.Results.Average(r => r.PriceEur);
                _logger.LogInformation($"Average price for listing {airbnbId}: {averagePrice}");

                return averagePrice;
            }
         
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting listing price");
            throw new Exception($"An error occurred while getting the listing price, {ex.Message}");
        }
    }


   
    private (int Year, int Month) GetPreviousMonth(int year, int currentMonth)
    {
        if (currentMonth == 1)
        {
            return (year - 1, 12);
        }
        return (year, currentMonth - 1);
    }

    private decimal CalculatePercentageDifference(decimal currentPrice, decimal previousPrice)
    {
        if (previousPrice == 0)
                {
                    if (currentPrice == 0)
                        return 0; // No change
                    else
                        return 100; // Full increase (or maybe 999 depending on your intent)
                }

        return ((currentPrice - previousPrice) / previousPrice) * 100;
    }



    public class AvailabilityResponse
    {
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("results")]
        public List<AvailabilityResult> Results { get; set; }
    }

    public class AvailabilityResult
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("closed_to_arrival")]
        public int ClosedToArrival { get; set; }

        [JsonPropertyName("closed_to_departure")]
        public int ClosedToDeparture { get; set; }

        [JsonPropertyName("available")]
        public int Available { get; set; }

        [JsonPropertyName("available_for_checkin")]
        public int AvailableForCheckin { get; set; }

        [JsonPropertyName("minNights")]
        public int MinNights { get; set; }

        [JsonPropertyName("maxNights")]
        public int MaxNights { get; set; }
    }

    public class PriceResponse
    {
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }

        [JsonPropertyName("results")]
        public List<PriceResult> Results { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }
    }

    public class PriceResult
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("price_eur")]
        public decimal PriceEur { get; set; }

        [JsonPropertyName("price_usd")]
        public decimal PriceUsd { get; set; }
    }
}
