using ABMB.Models;
using Microsoft.EntityFrameworkCore;

namespace ABMB.Properties;

public class AppDbContext : DbContext
{
    public DbSet<Hotel> Hotels { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\postgresql;Database=ABMB;Trusted_Connection=True;");
    }
}