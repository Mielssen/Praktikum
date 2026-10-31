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
                .Select(ToDto)
                .ToList();
        }

        public void Add(int credentialId, int tourId)
        {
            if (!_repo.TourExists(tourId))
                throw new AppException(ErrorCodes.NotFound, "Тур не знайдено");

            if (_repo.GetByCredentialAndTour(credentialId, tourId) != null)
                throw new AppException(ErrorCodes.AlreadyExists, "Тур вже в обраному");

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
                throw new AppException(ErrorCodes.NotFound, "Тур не в обраному");

            _repo.Delete(favorite);
        }

        private static FavoriteDto ToDto(Favorite f) => new()
        {
            FavoriteId = f.FavoriteId,
            TourId = f.TourId,
            TourName = f.Tour.Name,
            Description = f.Tour.Description,
            Price = f.Tour.Price,
            DurationDays = f.Tour.DurationDays,
            CreatedAt = f.CreatedAt
        };
    }
}