using ABMB.Properties;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using ABMB.Controllers.CSV;
using ABMB.Controllers;
using Microsoft.Extensions.Logging;

namespace ABMB;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var host = CreateHostBuilder(args).Build();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting application...");
            
            // Apply migrations at startup with retry
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var maxRetries = 5;
                var retryCount = 0;
                var delay = 5000; // 5 seconds

                while (retryCount < maxRetries)
                {
                    try
                    {
                        var context = services.GetRequiredService<AppDbContext>();
                        logger.LogInformation($"Attempting database connection (attempt {retryCount + 1}/{maxRetries})...");
                        context.Database.CanConnect();
                        logger.LogInformation("Database connection successful");
                        
                        logger.LogInformation("Applying database migrations...");
                        context.Database.Migrate();
                        logger.LogInformation("Database migrations completed successfully");
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        logger.LogWarning($"Database connection attempt {retryCount} failed: {ex.Message}");
                        
                        if (retryCount == maxRetries)
                        {
                            logger.LogError(ex, "Failed to connect to database after all retry attempts");
                            throw;
                        }
                        
                        logger.LogInformation($"Waiting {delay/1000} seconds before next attempt...");
                        Thread.Sleep(delay);
                    }
                }
            }

            logger.LogInformation("Application started successfully");
            host.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application failed to start: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            throw;
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
                    options.ListenAnyIP(8080);
                });

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
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configure services
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
        });

        services.AddCors(options =>
        {
            options.AddPolicy(("AllowAll"), builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

    services.AddDbContext<AppDbContext>(options =>
           options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
   
       services.AddDbContextFactory<AppDbContext>(options =>
           options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
        services.AddControllers();
        services.AddTransient<AirbnbCsvService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseStaticFiles();
        app.UseCors("AllowAll");
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}


//using ABMB.Controllers;
// using ABMB.Properties;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Options;

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.

// builder.Services.AddControllers();
// // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
// builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(8080); });

// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// builder.Services.AddScoped<CsvService>();
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseNpgsql(connectionString));

// // var csvFilePath = "./data/flights.csv";
// // builder.Services.AddSingleton<CsvService>(sp => new CsvService(csvFilePath));

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment()) app.MapOpenApi();

// //app.UseHttpsRedirection();
// app.UseRouting();

// app.UseAuthorization();

// app.MapControllers();

// app.Run();/ 