namespace ABMB.Models;

public class Flight
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public required string Rating { get; set; }
    public required string Price { get; set; }
    public required string Time { get; set; }
    public required string Date { get; set; }
    public required string Airline { get; set; }
    public required string Airport { get; set; }
    public required string Gate { get; set; }
}