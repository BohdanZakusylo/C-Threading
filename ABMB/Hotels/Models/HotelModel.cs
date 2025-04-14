using System;
using ABMB.Models;

namespace ABMB.Hotels
{
    public class HotelModel
    {
        public Hotel dbHotel { get; set; }
        public int? id { get; set; }
        public ApiHotelModel? ApiHotelModel { get; set; }
    }
}
