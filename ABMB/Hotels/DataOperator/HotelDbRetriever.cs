using ABMB.Models;
using ABMB.Properties;

namespace ABMB.Hotels
{
    public class HotelDbRetriever
    {
        private readonly AppDbContext _context;

        public HotelDbRetriever(AppDbContext context)
        {
            _context = context;
        }

        public List<Hotel> getHotelsDb(string destination)
        {
            try
            {
                List<Hotel> hotelsFromDb = _context
                    .Hotels.Where(h => h.cityName.Equals(destination))
                    .ToList();

                return hotelsFromDb;
            }
            catch (Exception e)
            {
                throw new CustomHotelException("Hotels were not found in the database");
            }
        }
    }
}
