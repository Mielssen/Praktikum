using Praktikum542.DTOs;
using Praktikum542.Exceptions;
using Praktikum542.Models;
using Praktikum542.Repositories;

namespace Praktikum542.Services
{
    public class ReviewService
    {
        private readonly ReviewRepository _repo;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            ReviewRepository repo,
            ILogger<ReviewService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public void Create(int credentialId, CreateReviewDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                throw new AppException(
                    "INVALID_RATING",
                    "Оцінка повинна бути від 1 до 5"
                );

            var booking = _repo.GetBookingById(dto.BookingId);

            if (booking == null)
                throw new AppException(
                    "NOT_FOUND",
                    "Бронювання не знайдено"
                );

            if (booking.CredentialId != credentialId)
                throw new AppException(
                    "FORBIDDEN",
                    "Це не ваше бронювання"
                );

            var tourEndDate =
                booking.StartDate.AddDays(booking.Tour.DurationDays);

            var today = DateOnly.FromDateTime(DateTime.Now);

            if (tourEndDate > today)
                throw new AppException(
                    "TOUR_NOT_COMPLETED",
                    "Відгук можна залишити лише після завершення туру"
                );

            if (_repo.ReviewExistsForBooking(dto.BookingId))
                throw new AppException(
                    "REVIEW_EXISTS",
                    "Відгук для цього бронювання вже залишено"
                );

            var review = new Review
            {
                CredentialId = credentialId,
                TourId = booking.TourId,
                BookingId = booking.BookingId,
                Rating = (sbyte)dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.Now
            };

            _repo.Add(review);

            _logger.LogInformation(
                "Створено відгук: UserID={UserId}, BookingID={BookingId}, TourID={TourId}, Rating={Rating}",
                credentialId,
                booking.BookingId,
                booking.TourId,
                dto.Rating
            );
        }

        public List<ReviewDto> GetByTourId(int tourId)
        {
            var reviews = _repo.GetByTourId(tourId);

            return reviews.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                BookingId = r.BookingId,
                TourId = r.TourId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UserName = r.Credential?.UserDetail?.Name
            }).ToList();
        }

        public double GetAverageRating(int tourId)
        {
            return _repo.GetAverageRating(tourId);
        }
    }
}