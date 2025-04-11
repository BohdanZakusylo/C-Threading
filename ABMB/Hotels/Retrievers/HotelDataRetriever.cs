using System.Text.Json;

namespace ABMB.Hotels
{
    public class HotelDataRetriever
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string apikey = "d7e04bf279mshec134d993d91779p127357jsn01575d5b549f";

        public async Task OperateHotelModel(List<HotelModel> lstHotelModel)
        {
            foreach (HotelModel hotelModel in lstHotelModel)
            {
                hotelModel.ApiHotelModel = await this.RetreiveHotelInfo(hotelModel.id!.Value);
            }
        }
        public async Task<ApiHotelModel> RetreiveHotelInfo(int HotelId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    $"https://booking-com15.p.rapidapi.com/api/v1/hotels/getHotelDetails?hotel_id={HotelId}&arrival_date=2025-11-12&departure_date=2025-11-14&adults=1&children_age=1%2C17&room_qty=1&units=metric&temperature_unit=c&languagecode=en-us&currency_code=EUR"
                ),
                Headers =
                {
                    { "x-rapidapi-key", apikey },
                    { "x-rapidapi-host", "booking-com15.p.rapidapi.com" },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                if (body == null)
                {
                    throw new Exception("hotel id is incorrect");
                }

                Console.WriteLine(HotelId);

                using JsonDocument doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var data = root.GetProperty("data");
                var url = data.GetProperty("url").GetString();
                var avialableRooms = data.GetProperty("available_rooms").GetInt32();
                var gross_amount = data.GetProperty("product_price_breakdown").GetProperty("gross_amount");
                var price = gross_amount.GetProperty("amount_rounded").GetString();
                var currency = gross_amount.GetProperty("currency").GetString();

                ApiHotelModel apiHotelModel = new()
                {
                    Price = price,
                    currency = currency,
                    Url = url,
                    available_rooms = avialableRooms
                };

                return apiHotelModel;
            }
        }
    }
}
