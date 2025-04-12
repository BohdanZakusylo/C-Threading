using ABMB.Models;
using ABMB.Properties;

namespace ABMB.Hotels
{
    public class HotelDataOperator
    {
        public string destination;
        public string arrivalDate;
        public string departureDate;
        private HotelDbRetriever hotelDbRetriever;

        public HotelDataOperator(string destination, AppDbContext context, string arrivalDate, string departureDate)
        {
            this.destination = destination;
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
                throw new CustomHotelException("City Was Not Found");
            }

            return cityId;
        }

        public async Task<List<HotelModel>> GetValidHotelIds()
        {
            HotelRetriever hotelRetriever = new();
            HotelDataRetriever hotelDataRetriever = new();

            string cityId = await this.GetCityId();
            List<Hotel> hotels = this.hotelDbRetriever.getHotelsDb(destination);
            List<HotelModel> readyHotels = await hotelRetriever.RetrieveHotelIds(cityId, hotels, this.arrivalDate, this.departureDate);

            await hotelDataRetriever.OperateHotelModel(readyHotels);

            return readyHotels;
        }
    }
}