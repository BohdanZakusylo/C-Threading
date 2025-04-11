namespace ABMB.Models;

public class Airbnb
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public double Rating { get; set; }
    public int Reviews { get; set; }
    public required string HostName { get; set; }
    public int HostId { get; set; }
    public required string Address { get; set; }
    public required string Features { get; set; }
    public required string Amenities { get; set; }
    public required string SafetyRules { get; set; }
    public required string HouseRules { get; set; }
    public required string ImgLinks { get; set; }
    public decimal Price { get; set; }
    public required string Country { get; set; }
    public int Bathrooms { get; set; }
    public int Beds { get; set; }
    public int Guests { get; set; }
    public int Toilets { get; set; }
    public int Bedrooms { get; set; }
    public int Studios { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}