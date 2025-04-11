namespace ABMB.Models;

public class OldFlight
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int DepTime { get; set; }
    public int SchedDepTime { get; set; }
    public int DepDelay { get; set; }
    public int ArrTime { get; set; }
    public int SchedArrTime { get; set; }
    public int ArrDelay { get; set; }
    public required string Carrier { get; set; }
    public int FlightNumber { get; set; }
    public required string TailNum { get; set; }
    public required string Origin { get; set; }
    public required string Destination { get; set; }
    public int AirTime { get; set; }
    public int Distance { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public required string TimeHour { get; set; }
    public required string Name { get; set; }
    public DateTime FlightDate { get; set; }
    public int DepartureTime { get; set; }
    public int ArrivalTime { get; set; }
}