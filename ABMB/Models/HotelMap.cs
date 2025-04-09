using CsvHelper.Configuration;
using ABMB.Models;

public sealed class HotelMap : ClassMap<Hotel>
{
    public HotelMap()
    {
        // Do not map Id – let the DB handle it
        Map(m => m.countyName).Name("countyName");
        Map(m => m.cityName).Name("cityName");
        Map(m => m.HotelName).Name("HotelName");
        Map(m => m.PhoneNumber).Name("PhoneNumber");
    }
}