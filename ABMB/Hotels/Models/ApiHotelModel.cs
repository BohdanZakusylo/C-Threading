using System;

namespace ABMB.Hotels
{
    public class ApiHotelModel
    {
        public string? Price { get; set; }
        public string? currency { get; set; }
        public string? Url { get; set; }
        public int? available_rooms { get; set; }
    }
}