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
                .Where(t => t.Name.Contains(query) || (t.Description != null && t.Description.Contains(query)))
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

        public (List<Tour> Items, int TotalCount) Filter(
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            int? minDays,
            int? maxDays,
            int? typeId,
            int page = 1,
            int pageSize = 6)
        {
            var query = _context.Tours
                .Include(t => t.TourAssets)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Name.Contains(search) || (t.Description != null && t.Description.Contains(search)));

            if (minPrice.HasValue)
                query = query.Where(t => t.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(t => t.Price <= maxPrice.Value);

            if (minDays.HasValue)
                query = query.Where(t => t.DurationDays >= minDays.Value);

            if (maxDays.HasValue)
                query = query.Where(t => t.DurationDays <= maxDays.Value);

            if (typeId.HasValue)
                query = query.Where(t => t.TypeId == typeId.Value);

            int totalCount = query.Count();

     
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 6;

            var items = query
                .OrderBy(t => t.TourId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, totalCount);
        }
    }
}