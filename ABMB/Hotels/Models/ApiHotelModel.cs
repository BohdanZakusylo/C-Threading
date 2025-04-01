using System;

namespace ABMB.Hotels
{
    public class ApiHotelModel
    {
        private int Price
        {
            get
            {
                return Price;
            }
            set
            {
                Price = (int)value;
            }
        }
        private string Url { get; set; }
        private double AvgRoomSize;
    }
}