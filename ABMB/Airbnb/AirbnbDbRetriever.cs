using ABMB.Models;
using ABMB.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABMB.Controllers.AirbnbModule;

public class AirbnbDbRetriever
{
    private readonly AppDbContext _context;

    public AirbnbDbRetriever(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<long>> GetAirbnbListings(string country, string city)
    {
        try
        {
            var foundListings = await _context
                .Airbnbs.Where(a => a.Country.ToLower().Trim() == country.ToLower().Trim())
                .ToListAsync();

            List<long> matchingIds = foundListings
                .Where(a =>
                    a.Address != null && a.Address.ToLower().Contains(city.ToLower().Trim())
                )
                .Select(a => a.AirbnbId)
                .ToList();

            return matchingIds;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
