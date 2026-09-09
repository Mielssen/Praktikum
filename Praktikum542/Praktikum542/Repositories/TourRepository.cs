using Microsoft.EntityFrameworkCore;
using Praktikum542.Models;

namespace Praktikum542.Repositories
{
    public class TourRepository
    {
        private readonly PraktikumContext _context;

        public TourRepository(PraktikumContext context)
        {
            _context = context;
        }

        public void Add(Tour tour)
        {
            _context.Tours.Add(tour);
            _context.SaveChanges();
        }
        public List<Tour> Search(string query)
        {
            return _context.Tours
                .Include(t => t.TourAssets)
                .Where(t => t.Name.Contains(query) || t.Description.Contains(query))
                .ToList();
        }
        public List<Tour> GetAll()
        {
            return _context.Tours
                .Include(t => t.TourAssets)
                .ToList();
        }

        public void Update(Tour tour)
        {
            _context.Tours.Update(tour);
            _context.SaveChanges();
        }

        public Tour GetById(int id)
        {
            return _context.Tours
                .Include(x => x.TourAssets)
                .FirstOrDefault(x => x.TourId == id);
        }

        public void Delete(Tour tour)
        {
            _context.Tours.Remove(tour);
            _context.SaveChanges();
        }
    }
}
