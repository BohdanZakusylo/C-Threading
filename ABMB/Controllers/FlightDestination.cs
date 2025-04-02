using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ABMB.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightDestination : ControllerBase
{
    [HttpGet("price-graph")]
    public async Task<IActionResult> GetPriceGraph(
            [FromQuery] string departure_id,
            [FromQuery] string arrival_id,
            [FromQuery] int adults,
            [FromQuery] string departure_date,
            [FromQuery] string return_date)
        {
            if (string.IsNullOrWhiteSpace(departure_id) || string.IsNullOrWhiteSpace(arrival_id)
                || string.IsNullOrWhiteSpace(departure_date) || string.IsNullOrWhiteSpace(return_date) || adults < 1)
            {
                return BadRequest("Missing or invalid query parameters.");
            }

        var uri = $"https://google-flights2.p.rapidapi.com/api/v1/searchFlights" +
                          $"?departure_id={departure_id}" +
                          $"&arrival_id={arrival_id}" +
                          $"&outbound_date={departure_date}" +
                          $"&travel_class=ECONOMY" +
                          $"&adults={adults}" +
                          $"&show_hidden=1&currency=USD&language_code=en-US&country_code=US";

        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("x-rapidapi-key", "0b58d697b5msh673289dc2c33c80p1cea81jsn704862362afb");
        request.Headers.Add("x-rapidapi-host", "google-flights2.p.rapidapi.com");

        try
        {
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return Ok(body);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, $"Error fetching flight data: {ex.Message}");
        }
    }
}
