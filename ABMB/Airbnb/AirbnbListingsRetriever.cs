using Microsoft.AspNetCore.Mvc;
using ABMB.Models;
using ABMB.Properties;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using ABMB.Controllers.AirbnbModule;


public class AirbnbListingsRetriever{

    private readonly AppDbContext _context;
    private readonly ILogger<AirbnbListingsRetriever> _logger;
    private readonly HttpClient _httpClient;
    private readonly AirbnbUtils _airbnbUtils;
    private const string RapidApiKey = "d4d0ef677fmsha3418e97ed26df9p12df8bjsn6c2d64ebd551";
    private const string RapidApiHost = "airbnb-listings.p.rapidapi.com";

    public AirbnbListingsRetriever(AppDbContext context, ILogger<AirbnbListingsRetriever> logger, IHttpClientFactory httpClientFactory, AirbnbUtils airbnbUtils){
        _context = context;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _airbnbUtils = airbnbUtils;
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
}
