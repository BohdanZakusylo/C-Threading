using CsvHelper.Configuration;

namespace ABMB.Models;

public class AirbnbMap : ClassMap<Airbnb>
{
    public AirbnbMap()
    {
        // Map CSV headers to class properties
        Map(m => m.IdAirbnb).Name("IdAirbnb");
        Map(m => m.Name).Name("name");
        Map(m => m.Rating).Name("rating").Default(0.0);
        Map(m => m.Reviews).Name("reviews").Default(0);
        Map(m => m.HostName).Name("host_name");
        Map(m => m.HostId).Name("host_id");
        Map(m => m.Address).Name("address");
        Map(m => m.Features).Name("features");
        Map(m => m.Amenities).Name("amenities");
        Map(m => m.SafetyRules).Name("safety_rules");
        Map(m => m.HouseRules).Name("house_rules");
        Map(m => m.ImgLinks).Name("img_links");
        Map(m => m.Price).Name("price");
        Map(m => m.Country).Name("country");
        Map(m => m.Bathrooms).Name("bathrooms").Default(0);
        Map(m => m.Beds).Name("beds").Default(0);
        Map(m => m.Guests).Name("guests").Default(0);
        Map(m => m.Toilets).Name("toilets").Default(0);
        Map(m => m.Bedrooms).Name("bedrooms").Default(0);
        Map(m => m.Studios).Name("studios").Default(0);
        Map(m => m.CheckIn).Name("check_in");
        Map(m => m.CheckOut).Name("check_out");
    }
}