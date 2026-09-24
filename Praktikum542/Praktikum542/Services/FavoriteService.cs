using Praktikum542.DTOs;
using Praktikum542.Exceptions;
using Praktikum542.Models;
using Praktikum542.Repositories;

namespace Praktikum542.Services
{
    public class FavoriteService
    {
        private readonly FavoriteRepository _repo;

        public FavoriteService(FavoriteRepository repo)
        {
            _repo = repo;
        }

        public List<FavoriteDto> GetMy(int credentialId)
        {
            return _repo.GetByCredentialId(credentialId)
                .Select(f => new FavoriteDto
                {
                    FavoriteId = f.FavoriteId,
                    TourId = f.TourId,
                    TourName = f.Tour.Name,
                    Description = f.Tour.Description,
                    Price = f.Tour.Price,
                    DurationDays = f.Tour.DurationDays,
                    CreatedAt = f.CreatedAt
                }).ToList();
        }

        public void Add(int credentialId, int tourId)
        {
            if (!_repo.TourExists(tourId))
                throw new AppException("NOT_FOUND", "Тур не знайдено");

            var existing = _repo.GetByCredentialAndTour(credentialId, tourId);
            if (existing != null)
                throw new AppException("ALREADY_EXISTS", "Тур вже в обраному");

            _repo.Add(new Favorite
            {
                CredentialId = credentialId,
                TourId = tourId,
                CreatedAt = DateTime.Now
            });
        }

        public void Delete(int credentialId, int tourId)
        {
            var favorite = _repo.GetByCredentialAndTour(credentialId, tourId);

            if (favorite == null)
                throw new AppException("NOT_FOUND", "Тур не в обраному");

            _repo.Delete(favorite);
        }
    }
}