namespace ABMB.Models;

public class Airbnb
{
    public int Id { get; set; }
    public long AirbnbId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Rating { get; set; } = "0"; // Default value
    public string Reviews { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string HostId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Features { get; set; } = string.Empty;
    public string Amenities { get; set; } = string.Empty;
    public string SafetyRules { get; set; } = string.Empty;
    public string HouseRules { get; set; } = string.Empty;
    public string ImgLinks { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Country { get; set; } = string.Empty;
    public int Bathrooms { get; set; }
    public int Beds { get; set; }
    public int Guests { get; set; }
    public int Toilets { get; set; }
    public int Bedrooms { get; set; }
    public int Studios { get; set; }
    public string CheckIn { get; set; } = "Flexible";
    public string CheckOut { get; set; } = "Flexible";
}
