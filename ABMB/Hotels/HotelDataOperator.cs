using ABMB.Models;
using ABMB.Properties;

namespace ABMB.Hotels
{
    public class HotelDataOperator
    {
        public string destination;
        public string arrivalDate;
        public string departureDate;
        private readonly AppDbContext _context;
        private HotelDbRetriever hotelDbRetriever;

        public HotelDataOperator(string destination, AppDbContext context, string arrivalDate, string departureDate)
        {
            this.destination = destination;
            this._context = context;
            this.hotelDbRetriever = new(context);
            this.arrivalDate = arrivalDate;
            this.departureDate = departureDate;
        }
        public async Task<string> GetCityId()
        {
            HotelDestinationRetriever hotelDestinationRetriever = new();
            string? cityId = await hotelDestinationRetriever.RetreiveDestination(this.destination);

            if (cityId == null)
            {
                throw new Exception("City Not Found");
            }

            return cityId;
        }

        public async Task GetValidHotelIds()
        {
            HotelRetriever hotelRetriever = new();
            HotelDataRetriever hotelDataRetriever = new();

            string cityId = await this.GetCityId();
            List<Hotel> hotels = this.hotelDbRetriever.getHotelsDb(destination);
            List<HotelModel> readyHotels = await hotelRetriever.RetrieveHotelIds(cityId, hotels, this.arrivalDate, this.departureDate);

            await hotelDataRetriever.OperateHotelModel(readyHotels);

            foreach (HotelModel hotelModel in readyHotels)
            {
                Console.WriteLine(hotelModel.id + " - " + hotelModel.dbHotel.HotelName + " - " + hotelModel.ApiHotelModel.Url);
            }
        }
    }
}