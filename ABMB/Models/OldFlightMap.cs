using CsvHelper.Configuration;

namespace ABMB.Models;

public class OldFlightMap : ClassMap<OldFlight>
{
    public OldFlightMap()
    {
        // Map CSV headers to class properties
        Map(m => m.Id).Name("id");
        Map(m => m.Year).Name("year");
        Map(m => m.Month).Name("month");
        Map(m => m.Day).Name("day");
        Map(m => m.DepTime).Name("dep_time").Default(0.0);
        Map(m => m.SchedDepTime).Name("sched_dep_time");
        Map(m => m.DepDelay).Name("dep_delay").Default(0.0);
        Map(m => m.ArrTime).Name("arr_time").Default(0.0);
        Map(m => m.SchedArrTime).Name("sched_arr_time");
        Map(m => m.ArrDelay).Name("arr_delay").Default(0.0);
        Map(m => m.Carrier).Name("carrier");
        Map(m => m.FlightNumber).Name("flight");
        Map(m => m.TailNum).Name("tailnum");
        Map(m => m.Origin).Name("origin");
        Map(m => m.Destination).Name("dest");
        Map(m => m.AirTime).Name("air_time").Default(0.0);
        Map(m => m.Distance).Name("distance");
        Map(m => m.Hour).Name("hour");
        Map(m => m.Minute).Name("minute");
        Map(m => m.TimeHour).Name("time_hour");
        Map(m => m.Name).Name("name");
    }
}