using Microsoft.EntityFrameworkCore;
using Praktikum542.Models;

namespace Praktikum542.Repositories
{
    public class ReviewRepository
    {
        private readonly PraktikumContext _context;

        public ReviewRepository(PraktikumContext context)
        {
            _context = context;
        }

        public Booking? GetBookingById(int bookingId)
        {
            return _context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefault(b => b.BookingId == bookingId);
        }

        public bool ReviewExistsForBooking(int bookingId)
        {
            return _context.Reviews
                .Any(r => r.BookingId == bookingId);
        }

        public void Add(Review review)
        {
            _context.Reviews.Add(review);
            _context.SaveChanges();
        }

        public List<Review> GetByTourId(int tourId)
        {
            return _context.Reviews
                .Include(r => r.Credential)
                    .ThenInclude(c => c.UserDetail)
                .Where(r => r.TourId == tourId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public double GetAverageRating(int tourId)
        {
            var ratings = _context.Reviews
                .Where(r => r.TourId == tourId)
                .Select(r => (double)r.Rating);

            return ratings.Any()
                ? Math.Round(ratings.Average(), 1)
                : 0;
        }
    }
}