using ABMB.Properties;
using ABMB.Models;

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
            List<Hotel> hotelsFromDb = _context.Hotels
            .Where(h => h.cityName.Equals(destination))
            .ToList();

            return hotelsFromDb;
        }
    }
}