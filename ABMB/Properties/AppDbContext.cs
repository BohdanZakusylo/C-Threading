using ABMB.Models;
using Microsoft.EntityFrameworkCore;

namespace ABMB.Properties;

public class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<OldFlight> OldFlights { get; set; }
    public DbSet<Flight> Flights { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseNpgsql(
                "Host=db;Port=5432;Database=abmbv2;Username=postgres;Password=nhlstenden2025"
            );
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseNpgsql(
                "Host=db;Port=5432;Database=abmbv2;Username=postgres;Password=nhlstenden2025"
            )
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
