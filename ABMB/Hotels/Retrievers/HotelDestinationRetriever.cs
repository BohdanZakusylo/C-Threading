using System;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ABMB.Hotels
{
    public class HotelDestinationRetriever()
    {
        private readonly string apiKey = Environment.GetEnvironmentVariable("RAPID_API_KEY")!;

        public async Task<string?> RetreiveDestination(string Destination)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    $"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchDestination?query={Destination.Replace(" ", "%20")}"
                ),
            };

            request.Headers.Add("x-rapidapi-key", apiKey);
            request.Headers.Add("x-rapidapi-host", "booking-com15.p.rapidapi.com");

            try
            {
                using (var client = new HttpClient())
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    using JsonDocument doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;
                    var dataArray = root.GetProperty("data");

                    foreach (var item in dataArray.EnumerateArray())
                    {
                        string searchType = item.GetProperty("search_type").GetString()!;
                        string city_name = item.GetProperty("city_name").GetString()!;

                        if (searchType == "city" && city_name.Equals(Destination))
                        {
                            Console.WriteLine(item.GetProperty("dest_id").GetString());
                            return item.GetProperty("dest_id").GetString();
                        }
                    }

                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new CustomHotelException("API key error");
            }
        }
    }
}
