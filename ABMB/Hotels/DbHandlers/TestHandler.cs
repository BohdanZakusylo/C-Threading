using System;

namespace ABMB.Hotels
{
    public class TestHandler
    {
        public List<HotelModel> HotelsModelList = [];

        public TestHandler()
        {
            HotelModel htlml1 = new HotelModel
            {
                CountryName = "Albania",
                CityName = "Berat",
                HotelName = "Hotel Demaj",
                HotelAddress = "123 Main Street",
                Description = "A beautiful hotel in the heart of the city.",
            };
            HotelModel htlml2 = new HotelModel
            {
                CountryName = "Ukraine",
                CityName = "Kyiv",
                HotelName = "Kyiv City Hotel",
                HotelAddress = "123 Main Street",
                Description = "A beautiful hotel in the heart of the city.",
            };

            HotelsModelList.Add(htlml1);
            HotelsModelList.Add(htlml2);
        }
    }
}