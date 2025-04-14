using ABMB.Properties;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

namespace ABMB;

public class Program
{
    public static void Main(string[] args)
    {
        // Load .env first
        DotNetEnv.Env.Load();

        var key = Environment.GetEnvironmentVariable("RAPID_API_KEY");

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(
                (hostingContext, config) =>
                {
                    DotNetEnv.Env.Load(); // Load .env again here to inject into IConfiguration

                    // Get all .env variables and inject into IConfiguration
                    var envVars = Environment.GetEnvironmentVariables();
                    var dict = new Dictionary<string, string?>();
                    foreach (var key in envVars.Keys)
                    {
                        var strKey = key?.ToString();
                        var value = envVars[key]?.ToString();
                        if (strKey != null && value != null)
                            dict[strKey] = value;
                    }

                    config.AddInMemoryCollection(dict);
                }
            )
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

// You'll also need to create a Startup class
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;

        // Option 1: via Environment
        var keyFromEnv = Environment.GetEnvironmentVariable("RAPID_API_KEY");
        Console.WriteLine("From ENV: " + keyFromEnv);

        // Option 2: via IConfiguration
        var keyFromConfig = configuration["RAPID_API_KEY"];
        Console.WriteLine("From IConfiguration: " + keyFromConfig);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configure services
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
        });

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
        );

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
        );

        services.AddControllers();
        services.AddTransient<CsvService>();
        services.AddTransient<AirbnbService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseStaticFiles();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
