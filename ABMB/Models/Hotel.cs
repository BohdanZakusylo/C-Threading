namespace ABMB.Models;

public class Hotel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string Rating { get; set; }
    public required string Price { get; set; }
}