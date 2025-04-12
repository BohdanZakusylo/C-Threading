using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;

namespace ABMB.Models;

using CsvHelper.Configuration;

public class AirbnbMap : ClassMap<Airbnb>
{
    public AirbnbMap()
    {
        // Map CSV headers to class properties
        Map(m => m.Id).Ignore();
        
        Map(m => m.AirbnbId).Name("airbnb_id");
        Map(m => m.Name).Name("name");
        Map(m => m.Rating).Name("rating");
        Map(m => m.Reviews).Name("reviews");
        Map(m => m.HostName).Name("host_name");
        Map(m => m.HostId).Name("host_id");
        Map(m => m.Address).Name("address");
        Map(m => m.Features).Name("features");
        Map(m => m.Amenities).Name("amenities");
        Map(m => m.SafetyRules).Name("safety_rules");
        Map(m => m.HouseRules).Name("hourse_rules");
        Map(m => m.ImgLinks).Name("img_links");
        Map(m => m.Price).Name("price");
        Map(m => m.Country).Name("country");
        Map(m => m.Bathrooms).Name("bathrooms");
        Map(m => m.Beds).Name("beds");
        Map(m => m.Guests).Name("guests");
        Map(m => m.Toilets).Name("toilets");
        Map(m => m.Bedrooms).Name("bedrooms");
        Map(m => m.Studios).Name("studios");
        Map(m => m.CheckIn).Name("checkin");
        Map(m => m.CheckOut).Name("checkout");
    }
}