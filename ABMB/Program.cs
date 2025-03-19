using ABMB.Controllers;
using ABMB.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(8080); });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<CsvService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// var csvFilePath = "./data/flights.csv";
// builder.Services.AddSingleton<CsvService>(sp => new CsvService(csvFilePath));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();