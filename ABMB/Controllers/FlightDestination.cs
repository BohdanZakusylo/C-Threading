using DotNetEnv;
using Microsoft.AspNetCore.Mvc;

namespace ABMB.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightDestination : ControllerBase
{
    [HttpGet]
    [Route("price-graph")]
    public async Task<IActionResult> GetPRiceGraph()
    {
        Env.Load();
        var apikey = Environment.GetEnvironmentVariable("RAPIDAPI_KEY");
        var host = Environment.GetEnvironmentVariable("RAPIDAPI_HOST");

        using var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri =
                new Uri(
                    "https://google-flights2.p.rapidapi.com/api/v1/searchFlights?departure_id=LAX&arrival_id=JFK&travel_class=ECONOMY&adults=1&show_hidden=1&currency=USD&language_code=en-US&country_code=US"),
            Headers =
            {
                { "x-rapidapi-key", "12d042b15dmsh7a0c47e4051d94cp156254jsn787183a1d46f" },
                { "x-rapidapi-host", "google-flights2.p.rapidapi.com" }
            }
        };

        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            return Ok(body);
        }
    }
}