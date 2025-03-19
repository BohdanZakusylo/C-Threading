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
    public string Carrier { get; set; }
    public int FlightNumber { get; set; }
    public string TailNum { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public int AirTime { get; set; }
    public int Distance { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public string TimeHour { get; set; }
    public string Name { get; set; }
}