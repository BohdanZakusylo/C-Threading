using ABMB.Models;
using Microsoft.EntityFrameworkCore;

namespace ABMB.Properties;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Taxi> Taxis { get; set; }
    public DbSet<OldFlight> OldFlights { get; set; }
    public DbSet<Flight> Flights { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=abmbv2;Username=postgres;Password=nhlstenden2025");
    }
}
