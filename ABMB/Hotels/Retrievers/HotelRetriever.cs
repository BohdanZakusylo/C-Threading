using System.Net.Http.Headers;
using System.Text.Json;
using ABMB.Models;

namespace ABMB.Hotels
{
    public class HotelRetriever
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string apiKey = "d7e04bf279mshec134d993d91779p127357jsn01575d5b549f";
        public int Levenshtein(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) return b.Length;
            if (string.IsNullOrEmpty(b)) return a.Length;

            var d = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[a.Length, b.Length];
        }
        public async Task<List<HotelModel>> RetrieveHotelIds(string id, List<Hotel> hotelsFromDb, string arrivalDate, string departureDate)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    $"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchHotels?dest_id={id}&search_type=CITY&arrival_date={arrivalDate}&departure_date={departureDate}&adults=1&children_age=0%2C17&room_qty=1&page_number=1&units=metric&temperature_unit=c&languagecode=en-us&currency_code=EUR"
                ),
                Headers =
                {
                    { "x-rapidapi-key", apiKey },
                    { "x-rapidapi-host", "booking-com15.p.rapidapi.com" },
                },
            };

            List<HotelModel> lstHotelModel = new();

            using (var response = await client.SendAsync(request))
            {
                var body = "";
                try
                {
                    response.EnsureSuccessStatusCode();
                    body = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    throw new CustomHotelException("Incorrect Hotel Data");
                }

                using JsonDocument doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var data = root.GetProperty("data");
                var hotelArrays = data.GetProperty("hotels");

                foreach (var hotel in hotelArrays.EnumerateArray())
                {
                    var hotelProperty = hotel.GetProperty("property");
                    var name = hotelProperty.GetProperty("name").GetString()!;
                    var normalizedApiName = name.ToLowerInvariant().Trim();

                    foreach (Hotel dbHotel in hotelsFromDb)
                    {
                        var normalizedDbName = dbHotel.HotelName.ToLowerInvariant().Trim();
                        int distance = Levenshtein(normalizedApiName, normalizedDbName);

                        if (distance <= 6) // not very accurate but still
                        {
                            HotelModel hotelModel = new()
                            {
                                dbHotel = dbHotel,
                                id = hotel.GetProperty("hotel_id").GetInt32()
                            };

                            lstHotelModel.Add(hotelModel);
                            break;
                        }
                    }

                }

                return lstHotelModel;
            }
        }
    }
}

//will look into that, it is better
// using System.Net.Http.Headers;
// using System.Text.Json;
// using ABMB.Models;

// namespace ABMB.Hotels
// {
//     public class HotelRetriever
//     {
//         private readonly HttpClient client = new HttpClient();

//         public async Task<List<int>> RetrieveHotelIds(string id, List<Hotel> hotelsFromDb)
//         {
//             var request = new HttpRequestMessage
//             {
//                 Method = HttpMethod.Get,
//                 RequestUri = new Uri(
//                     $"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchHotels?dest_id={id}&search_type=CITY&arrival_date=2025-11-12&departure_date=2025-12-14&adults=1&children_age=0%2C17&room_qty=1&page_number=1&units=metric&temperature_unit=c&languagecode=en-us&currency_code=EUR"
//                 ),
//                 Headers =
//                 {
//                     { "x-rapidapi-key", "" },
//                     { "x-rapidapi-host", "booking-com15.p.rapidapi.com" },
//                 },
//             };

//             List<int> hotelIds = [];

//             using (var response = await client.SendAsync(request))
//             {
//                 response.EnsureSuccessStatusCode();
//                 var body = await response.Content.ReadAsStringAsync();

//                 using JsonDocument doc = JsonDocument.Parse(body);
//                 var root = doc.RootElement;
//                 var data = root.GetProperty("data");
//                 var hotelArrays = data.GetProperty("hotels");

//                 hotelIds = hotelArrays.EnumerateArray()
//                     .AsParallel()
//                     .Where(hotel =>
//                     {
//                         var hotelProperty = hotel.GetProperty("property");
//                         var name = hotelProperty.GetProperty("name").GetString()!;
//                         return hotelsFromDb.Any(dbHotel => dbHotel.HotelName.Equals(name));
//                     })
//                     .Select(hotel => hotel.GetProperty("hotel_id").GetInt32())
//                     .ToList();
//             }

//             return hotelIds;
//         }
//     }
// }
