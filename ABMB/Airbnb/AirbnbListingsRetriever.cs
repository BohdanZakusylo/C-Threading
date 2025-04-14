using System.Dynamic;
using System.Text.Json;
using ABMB.Controllers.AirbnbModule;
using ABMB.Models;
using ABMB.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AirbnbListingsRetriever
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly AirbnbUtils _airbnbUtils;
    private readonly string RapidApiKey = Environment.GetEnvironmentVariable("RAPID_API_KEY")!;
    private const string RapidApiHost = "airbnb-listings.p.rapidapi.com";

    public AirbnbListingsRetriever(AppDbContext context)
    {
        _context = context;
        _httpClient = new HttpClient();
        _airbnbUtils = new AirbnbUtils();
    }

    public async Task<List<ExpandoObject>> RetrieveDataForListings(long airbnbId)
    {
        try
        {
            var foundListings = await _context
                .Airbnbs.Where(a => a.AirbnbId == airbnbId)
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
                    a.CheckOut,
                })
                .ToListAsync();

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
            throw new Exception("No listings found matching the criteria");
        }
    }

    public async Task<bool> CheckAvailabilityForId(
        string airbnbId,
        int year,
        string month,
        string date
    )
    {
        try
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    $"https://{RapidApiHost}/v2/listingavailability?id={airbnbId}&year={year}&month={month}"
                ),
                Headers =
                {
                    { "X-RapidAPI-Key", RapidApiKey },
                    { "X-RapidAPI-Host", RapidApiHost },
                },
            };

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("api key error");
                return false;
            }

            var availabilityData = JsonSerializer.Deserialize<AvailabilityResponse>(content);
            if (availabilityData?.Results == null)
            {
                return false;
            }

            var availability = availabilityData.Results.FirstOrDefault(r => r.Date == date);
            if (availability == null)
            {
                return false;
            }

            return availability.Available == 1;
        }
        catch (Exception ex)
        {

            Console.WriteLine(ex);
            return false;
        }
    }

    public async Task<List<string>> CheckMultipleListingsAvailability(
        List<string> ids,
        int year,
        string month,
        string date
    )
    {
        var availableIds = new List<string>();

        try
        {
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
                    continue;
                }
            }
            return availableIds;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
