namespace ABMB.Hotels;

public class HotelModel
{
    public string? CountryName { get; set; }
    public string? CityName { get; set; }
    public string? HotelName { get; set; }
    public string? HotelAddress { get; set; }
    public string? Description { get; set; }
    public ApiHotelModel? ApiHotelModel { get; set; }
}