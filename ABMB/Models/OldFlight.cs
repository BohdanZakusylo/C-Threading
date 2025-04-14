namespace ABMB.Models;

public class OldFlight
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public double DepTime { get; set; } // Changed to double
    public int SchedDepTime { get; set; }
    public double DepDelay { get; set; } // Changed to double
    public double ArrTime { get; set; } // Changed to double
    public int SchedArrTime { get; set; }
    public double ArrDelay { get; set; } // Changed to double
    public string Carrier { get; set; }
    public int FlightNumber { get; set; }
    public string TailNum { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public double AirTime { get; set; } // Changed to double
    public int Distance { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public string TimeHour { get; set; }
    public string Name { get; set; }
}
