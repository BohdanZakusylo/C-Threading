using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using ABMB.Controllers.AirbnbModule;
using ABMB.Models;
using ABMB.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AirBnBPriceRetriever
{
    private readonly ILogger<AirBnBPriceRetriever> _logger;
    private readonly HttpClient _httpClient;
    private readonly AirbnbUtils _airbnbUtils;
    private readonly string RapidApiKey = Environment.GetEnvironmentVariable("RAPID_API_KEY")!;
    private const string RapidApiHost = "airbnb-listings.p.rapidapi.com";

    public AirBnBPriceRetriever(
        ILogger<AirBnBPriceRetriever> logger,
        IHttpClientFactory httpClientFactory,
        AirbnbUtils airbnbUtils
    )
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _airbnbUtils = airbnbUtils;
    }

    public async Task<decimal> GetPriceComparison(string airbnbId, int year, string month)
    {
        try
        {
            var currentPrice = await GetListingPrice(airbnbId, year, month);
            if (currentPrice == 0)
            {
                _logger.LogWarning(
                    $"Current price is 0 for listing comparison. airbnbId: {airbnbId}"
                );
            }

            var (previousYear, previousMonth) = _airbnbUtils.GetPreviousMonth(
                year,
                int.Parse(month)
            );

            var previousPrice = await GetListingPrice(
                airbnbId,
                previousYear,
                previousMonth.ToString("D2")
            );
            if (previousPrice == 0)
            {
                _logger.LogWarning(
                    $"Previous price is 0 for listing comparison. currentPrice: {currentPrice}"
                );
            }

            _logger.LogWarning(
                $"Previous price is 0 for listing comparison. currentPrice: {currentPrice}"
            );

            decimal percentageDifference = _airbnbUtils.CalculatePercentageDifference(
                currentPrice,
                previousPrice
            );
            _logger.LogInformation($"Percentage difference: {percentageDifference}");

            return percentageDifference;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error comparing prices for listing {airbnbId}");
            throw new Exception($"An error occurred while comparing prices, {ex.Message}");
        }
    }

    public async Task<decimal> GetListingPrice(string airbnbId, int year, string month)
    {
        try
        {
            _logger.LogInformation($"Getting price for listing {airbnbId} in {month} {year}");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    $"https://{RapidApiHost}/v2/listingPrices?id={airbnbId}&year={year}&month={month}"
                ),
                Headers =
                {
                    { "X-RapidAPI-Key", RapidApiKey },
                    { "X-RapidAPI-Host", RapidApiHost },
                },
            };

            using (var response = await _httpClient.SendAsync(request))
            {
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        $"RapidAPI request failed with status code: {response.StatusCode}"
                    );
                    throw new Exception($"Failed to get listing price, {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw response: {Response}", content);
                var priceData = JsonSerializer.Deserialize<PriceResponse>(content);

                if (priceData?.Results == null || !priceData.Results.Any())
                {
                    throw new Exception("No price data available for the specified period");
                }

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
}
