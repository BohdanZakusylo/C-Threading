using Microsoft.AspNetCore.Mvc;
using ABMB.Models;
using ABMB.Properties;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

using DotNetEnv;
Env.Load();

namespace ABMB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirbnbController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AirbnbController> _logger;
        private readonly HttpClient _httpClient;
        private const string RapidApiKey = "651054bd2emsh75e775ca77e0f05p16fbb3jsnf0a2fdc847f0"; // Replace with your actual RapidAPI key
        private const string RapidApiHost = "airbnb-listings.p.rapidapi.com";

        public AirbnbController(AppDbContext context, ILogger<AirbnbController> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", RapidApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", RapidApiHost);
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<int>>> SearchListings([FromQuery] string country, [FromQuery] string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(country))
                {
                    return BadRequest("Country parameter is required");
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    return BadRequest("Search key parameter is required");
                }

                _logger.LogInformation($"Searching for listings in {country} with keyword: {key}");

                // First filter by country
                var countryListings = await _context.Airbnbs
                    .Where(a => a.Country.Equals(country, StringComparison.OrdinalIgnoreCase))
                    .ToListAsync();

                // Then filter by keyword in address
                var matchingIds = countryListings
                    .Where(a => a.Address.Contains(key, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.Id)
                    .ToList();

                _logger.LogInformation($"Found {matchingIds.Count} matching listings");

                return Ok(matchingIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching listings");
                return StatusCode(500, "An error occurred while searching listings");
            }
        }

      

        [HttpGet("availability")]
        public async Task<ActionResult<List<string>>> CheckMultipleListingsAvailability(
            [FromQuery] List<string> ids, 
            [FromQuery] int year, 
            [FromQuery] string month, 
            [FromQuery] string date)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return BadRequest("At least one listing ID is required");
                }

                if (year < DateTime.Now.Year)
                {
                    return BadRequest("Year cannot be in the past");
                }

                if (string.IsNullOrWhiteSpace(month))
                {
                    return BadRequest("Month is required");
                }

                _logger.LogInformation($"Checking availability for {ids.Count} listings in {month} {year}");

                var availableIds = new List<string>();
                var tasks = ids.Select(id => CheckAvailabilityForId(id, year, month, date));
                var results = await Task.WhenAll(tasks);

                for (int i = 0; i < ids.Count; i++)
                {
                    if (results[i].Result is OkObjectResult okResult && okResult.Value is bool isAvailable && isAvailable)
                    {
                        availableIds.Add(ids[i]);
                    }
                }

                _logger.LogInformation($"Found {availableIds.Count} available listings out of {ids.Count}");
                return Ok(availableIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking multiple listings availability");
                return StatusCode(500, "An error occurred while checking availability");
            }
        }

        [HttpGet("availability/{id}")]
        public async Task<ActionResult<bool>> CheckAvailabilityForId(
            [FromRoute] string id,
            [FromQuery] int year,
            [FromQuery] string month,
            [FromQuery] string date)
        {
            try
            {
                var requestUri = $"https://{RapidApiHost}/v2/listingavailability?id={id}&year={year}&month={month}";
                var response = await _httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to check availability for listing {id}");
                    return StatusCode((int)response.StatusCode, "Failed to check availability");
                }

                var content = await response.Content.ReadAsStringAsync();
                var availabilityData = JsonSerializer.Deserialize<AvailabilityResponse>(content);

                if (availabilityData?.Results == null || !availabilityData.Results.Any())
                {
                    _logger.LogWarning($"No availability data found for listing {id}");
                    return NotFound("No availability data found");
                }

                // Find the specific date in the results
                var dateAvailability = availabilityData.Results.FirstOrDefault(r => r.Date == date);
                
                if (dateAvailability == null)
                {
                    _logger.LogWarning($"No availability data found for date {date} in listing {id}");
                    return NotFound($"No availability data found for date {date}");
                }

                // Return true if available is 1, false otherwise
                return Ok(dateAvailability.Available == 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking availability for listing {id}");
                return StatusCode(500, "An error occurred while checking availability");
            }
        }

        public async Task<ActionResult<decimal>> GetListingPrice(string id, int year, string month)
        {
            try
            {
                _logger.LogInformation($"Getting price for listing {id} in {month} {year}");

                var requestUri = $"https://{RapidApiHost}/v2/listingPrices?id={id}&year={year}&month={month}";
                var response = await _httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"RapidAPI request failed with status code: {response.StatusCode}");
                    return StatusCode((int)response.StatusCode, "Failed to get listing price");
                }

                var content = await response.Content.ReadAsStringAsync();
                var priceData = JsonSerializer.Deserialize<PriceResponse>(content);

                if (priceData?.Results == null || !priceData.Results.Any())
                {
                    return NotFound("No price data available for the specified period");
                }

                // Calculate average price
                var averagePrice = priceData.Results.Average(r => r.Price);
                _logger.LogInformation($"Average price for listing {id}: {averagePrice}");

                return Ok(averagePrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting listing price");
                return StatusCode(500, "An error occurred while getting the listing price");
            }
        }

        [HttpGet("price-comparison/{id}")]
        public async Task<ActionResult<decimal>> GetPriceComparison(
            [FromRoute] string id,
            [FromQuery] int year,
            [FromQuery] string month)
        {
            try
            {
                // Get current month's price
                var currentPrice = await GetPriceAsync(id, year, month, "current");
                if (!currentPrice.HasValue)
                {
                    return StatusCode(500, "Failed to get current month's price");
                }

                // Calculate previous month
                var (previousYear, previousMonth) = GetPreviousMonth(year, int.Parse(month));

                // Get previous month's price
                var previousPrice = await GetPriceAsync(id, previousYear, previousMonth.ToString("D2"), "previous");
                if (!previousPrice.HasValue)
                {
                    return StatusCode(500, "Failed to get previous month's price");
                }

                // Calculate percentage difference
                decimal percentageDifference = CalculatePercentageDifference(currentPrice.Value, previousPrice.Value);
                return Ok(percentageDifference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error comparing prices for listing {id}");
                return StatusCode(500, "An error occurred while comparing prices");
            }
        }

        private async Task<decimal?> GetPriceAsync(string id, int year, string month, string priceType)
        {
            var result = await GetListingPrice(id, year, month);
            return result.Result is OkObjectResult okResult && okResult.Value is decimal price 
                ? price 
                : null;
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
            return ((currentPrice - previousPrice) / previousPrice) * 100;
        }

        public class AvailabilityResponse
        {
            public required string RequestId { get; set; }
            public required List<AvailabilityResult> Results { get; set; }
        }

        public class AvailabilityResult
        {
            public required string Date { get; set; }
            public int ClosedToArrival { get; set; }
            public int ClosedToDeparture { get; set; }
            public int Available { get; set; }
            public int AvailableForCheckin { get; set; }
            public int MinNights { get; set; }
            public int MaxNights { get; set; }
        }

        public class PriceResponse
        {
            public required string RequestId { get; set; }
            public required List<PriceResult> Results { get; set; }
            public required string Currency { get; set; }
        }

        public class PriceResult
        {
            public required string Date { get; set; }
            public decimal Price { get; set; }
            public decimal PriceEur { get; set; }
            public decimal PriceUsd { get; set; }
        }
    }
}
