using System;
using System.Net.Http.Headers;

namespace ABMB.Hotels
{
    public class HotelDataRetriever
    {
        private readonly HttpClient client = new HttpClient();
        public async Task RetreiveHotelInfo(string HotelId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://booking-com15.p.rapidapi.com/api/v1/hotels/getHotelDetails?hotel_id={HotelId}&arrival_date=2025-11-12&departure_date=2025-11-14&adults=1&children_age=1%2C17&room_qty=1&units=metric&temperature_unit=c&languagecode=en-us&currency_code=EUR"),
                Headers =
                {
                    { "x-rapidapi-key", "" },
                    { "x-rapidapi-host", "booking-com15.p.rapidapi.com" },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                Console.WriteLine(body);
            }
        }
    }
}