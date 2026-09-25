using Microsoft.EntityFrameworkCore;
using Praktikum542.Models;

namespace Praktikum542.Repositories
{
    public class FavoriteRepository
    {
        private readonly PraktikumContext _context;

        public FavoriteRepository(PraktikumContext context)
        {
            _context = context;
        }

        public List<Favorite> GetByCredentialId(int credentialId)
        {
            return _context.Favorites
                .Include(f => f.Tour)
                .Where(f => f.CredentialId == credentialId)
                .ToList();
        }

        public Favorite? GetByCredentialAndTour(int credentialId, int tourId)
        {
            return _context.Favorites
                .FirstOrDefault(f => f.CredentialId == credentialId && f.TourId == tourId);
        }

        public bool TourExists(int tourId)
        {
            return _context.Tours.Any(t => t.TourId == tourId);
        }

        public void Add(Favorite favorite)
        {
            _context.Favorites.Add(favorite);
            _context.SaveChanges();
        }

        public void Delete(Favorite favorite)
        {
            _context.Favorites.Remove(favorite);
            _context.SaveChanges();
        }
    }
}