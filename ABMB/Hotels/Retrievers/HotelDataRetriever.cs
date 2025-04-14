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
        public async Task<ApiHotelModel?> RetreiveHotelInfo(int HotelId)
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
                    return null;
                }

                using JsonDocument doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                string? url = null;
                int? availableRooms = null;
                string? price = null;
                string? currency = null;

                if (root.TryGetProperty("data", out var data))
                {
                    if (data.TryGetProperty("url", out var urlProp))
                        url = urlProp.GetString();

                    if (data.TryGetProperty("available_rooms", out var roomsProp) && roomsProp.TryGetInt32(out var r))
                        availableRooms = r;

                    if (data.TryGetProperty("product_price_breakdown", out var ppb) &&
                        ppb.TryGetProperty("gross_amount", out var ga))
                    {
                        if (ga.TryGetProperty("amount_rounded", out var priceProp))
                            price = priceProp.GetString();

                        if (ga.TryGetProperty("currency", out var currencyProp))
                            currency = currencyProp.GetString();
                    }
                }

                ApiHotelModel apiHotelModel = new()
                {
                    Price = price,
                    currency = currency,
                    Url = url,
                    available_rooms = availableRooms
                };

                return apiHotelModel;
            }
        }
    }
}
