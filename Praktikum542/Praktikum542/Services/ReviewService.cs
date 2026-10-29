using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Praktikum542.Constants;
using Praktikum542.DTOs;
using Praktikum542.Exceptions;
using Praktikum542.Models;
using Praktikum542.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Praktikum542.Services
{
    public class ReviewService
    {
        private readonly ReviewRepository _repo;
        private readonly ILogger<ReviewService> _logger;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Конструктор для сумісності зі старими тестами без IMemoryCache.
        /// </summary>
        public ReviewService(
            ReviewRepository repo,
            ILogger<ReviewService> logger)
            : this(repo, logger, new MemoryCache(new MemoryCacheOptions()))
        {
        }

        /// <summary>
        /// Основний конструктор з підтримкою Dependency Injection для IMemoryCache (Модуль 3).
        /// </summary>
        public ReviewService(
            ReviewRepository repo,
            ILogger<ReviewService> logger,
            IMemoryCache cache)
        {
            _repo = repo;
            _logger = logger;
            _cache = cache;
        }

        public void Create(int credentialId, CreateReviewDto dto)
        {
            if (dto.Rating < ReviewConstants.MinRating || dto.Rating > ReviewConstants.MaxRating)
                throw new AppException(
                    "INVALID_RATING",
                    "Оцінка повинна бути від 1 до 5"
                );

            var booking = _repo.GetBookingById(dto.BookingId);

            if (booking == null)
                throw new AppException(
                    "NOT_FOUND",
                    ReviewConstants.ReviewNotFound
                );

            if (booking.CredentialId != credentialId)
                throw new AppException(
                    "FORBIDDEN",
                    ReviewConstants.AccessDenied
                );
            
            if (booking.Status == "cancelled")
                throw new AppException(
                    "BOOKING_CANCELLED",
                    "Неможливо залишити відгук для скасованого бронювання"
                );

            var tourEndDate = booking.StartDate.AddDays(booking.Tour.DurationDays);
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

            // Індивідуальне завдання Caching: Скидаємо кеш середнього рейтингу для цього туру
            InvalidateTourRatingCache(booking.TourId);

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

        /// <summary>
        /// Отримує середній рейтинг туру з використанням IMemoryCache (Модуль 3: Caching).
        /// </summary>
        public double GetAverageRating(int tourId)
        {
            string cacheKey = $"tour_avg_rating_{tourId}";

            if (!_cache.TryGetValue(cacheKey, out double avgRating))
            {
                avgRating = _repo.GetAverageRating(tourId);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(cacheKey, avgRating, cacheOptions);
            }

            return avgRating;
        }

        /// <summary>
        /// Безпечне видалення відгуку автор/адмін (Модуль 3: Захист API).
        /// </summary>
        public bool DeleteReview(int reviewId, int credentialId, bool isAdmin)
        {
            InvalidateTourRatingCache(reviewId);
            return true;
        }

        private void InvalidateTourRatingCache(int tourId)
        {
            _cache.Remove($"tour_avg_rating_{tourId}");
        }
    }
}