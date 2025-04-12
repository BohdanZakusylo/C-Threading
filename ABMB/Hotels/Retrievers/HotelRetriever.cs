using System.Text.Json;

namespace ABMB.Hotels;

public class HotelRetriever
{
    private readonly HttpClient client = new();

    private readonly List<string> lstfromDb =
    [
        "Kyiv City Hotel",
        "Ramada Encore Kyiv",
        "Central Dayflat Apartments",
        "Ukraine Hotel"
    ];

    public async Task<List<int>> RetrieveHotelIds(string id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(
                $"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchHotels?dest_id={id}&search_type=CITY&arrival_date=2025-11-12&departure_date=2025-12-14&adults=1&children_age=0%2C17&room_qty=1&page_number=1&units=metric&temperature_unit=c&languagecode=en-us&currency_code=EUR"
            ),
            Headers =
            {
                { "x-rapidapi-key", "" },
                { "x-rapidapi-host", "booking-com15.p.rapidapi.com" }
            }
        };

        List<int> hotelIds = [];

        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var data = root.GetProperty("data");
            var hotelArrays = data.GetProperty("hotels");

            foreach (var hotel in hotelArrays.EnumerateArray())
            {
                var hotelProperty = hotel.GetProperty("property");
                var name = hotelProperty.GetProperty("name").GetString()!;

                if (lstfromDb.Contains(name)) hotelIds.Add(hotel.GetProperty("hotel_id").GetInt32());
            }
        }

        return hotelIds;
    }
}