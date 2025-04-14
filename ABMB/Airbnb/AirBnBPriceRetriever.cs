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
    private readonly HttpClient _httpClient;
    private readonly AirbnbUtils _airbnbUtils;
    private readonly string RapidApiKey = Environment.GetEnvironmentVariable("RAPID_API_KEY")!;
    private const string RapidApiHost = "airbnb-listings.p.rapidapi.com";

    public AirBnBPriceRetriever(
    )
    {
        _httpClient = new HttpClient();
        _airbnbUtils = new AirbnbUtils();
    }

    public async Task<decimal> GetPriceComparison(string airbnbId, int year, string month)
    {
        try
        {
            var currentPrice = await GetListingPrice(airbnbId, year, month);

            var (previousYear, previousMonth) = _airbnbUtils.GetPreviousMonth(
                year,
                int.Parse(month)
            );

            var previousPrice = await GetListingPrice(
                airbnbId,
                previousYear,
                previousMonth.ToString("D2")
            );

            decimal percentageDifference = _airbnbUtils.CalculatePercentageDifference(
                currentPrice,
                previousPrice
            );

            return percentageDifference;
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while comparing prices, {ex.Message}");
        }
    }

    public async Task<decimal> GetListingPrice(string airbnbId, int year, string month)
    {
        try
        {
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

                    throw new Exception($"Failed to get listing price, {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var priceData = JsonSerializer.Deserialize<PriceResponse>(content);

                if (priceData?.Results == null || !priceData.Results.Any())
                {
                    throw new Exception("No price data available for the specified period");
                }

                var averagePrice = priceData.Results.Average(r => r.PriceEur);

                return averagePrice;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while getting the listing price, {ex.Message}");
        }
    }
}
