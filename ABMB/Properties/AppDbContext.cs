using ABMB.Models;
using k8s.KubeConfigModels;
using Microsoft.EntityFrameworkCore;

namespace ABMB.Properties;

public class AppDbContext : DbContext
{
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Taxi> Taxis { get; set; }
    public DbSet<Flight> Flights { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=admb;Username=postgres;Password=nhlstenden2025");
    }

}
