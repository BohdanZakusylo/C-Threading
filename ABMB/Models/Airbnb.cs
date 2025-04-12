namespace ABMB.Models;

public class Airbnb
{
    public int IdAirbnb { get; set; }
    public string Name { get; set; }
    public double Rating { get; set; }
    public int Reviews { get; set; }
    public string HostName { get; set; }
    public int HostId { get; set; }
    public string Address { get; set; }
    public string Features { get; set; }
    public string Amenities { get; set; }
    public string SafetyRules { get; set; }
    public string HouseRules { get; set; }
    public string ImgLinks { get; set; }
    public decimal Price { get; set; }
    public string Country { get; set; }
    public int Bathrooms { get; set; }
    public int Beds { get; set; }
    public int Guests { get; set; }
    public int Toilets { get; set; }
    public int Bedrooms { get; set; }
    public int Studios { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}