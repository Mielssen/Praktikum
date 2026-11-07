using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Exceptions;

namespace Praktikum542.Services
{
    public class TourService
    {

        private const string NotFoundCode = "NOT_FOUND";
        private const string VideoAssetType = "video";

        private readonly TourRepository _repo;
        private readonly PraktikumContext _context;
        private readonly ILogger<TourService> _logger;

        public TourService(TourRepository repo, PraktikumContext context, ILogger<TourService> logger)
        {
            _repo = repo;
            _context = context;
            _logger = logger;
        }

        public int CreateTour(CreateTourDto dto)
        {
            if (dto == null)
                throw new AppException("NULL", "Дані не передані");

            var typeExists = _context.TourTypes.Any(t => t.TypeId == dto.TypeId);
            if (!typeExists)
                throw new AppException(NotFoundCode, "Тип туру не знайдено");

            var tour = new Tour
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                Price = dto.Price,
                DurationDays = dto.DurationDays,
                AvailableFrom = dto.AvailableFrom,
                AvailableTo = dto.AvailableTo,
                TypeId = dto.TypeId
            };

            _repo.Add(tour);
            _logger.LogInformation("Створено тур: Name={Name}, Price={Price}", tour.Name, tour.Price);
            return tour.TourId;
        }

        private void UpdateTourAssets(int id, List<TourAssetDto>? assets)
        {
            var oldAssets = _context.TourAssets
                .Where(x => x.TourId == id)
                .ToList();

            var newUrls = assets?.Select(a => a.Url).ToHashSet()
                ?? new HashSet<string>();

            foreach (var asset in oldAssets)
            {
                if (!newUrls.Contains(asset.Url))
                {
                    DeleteAssetFile(asset);
                }
            }

            _context.TourAssets.RemoveRange(oldAssets);

            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    _context.TourAssets.Add(new TourAsset
                    {
                        TourId = id,
                        AssetType = asset.AssetType,
                        Url = asset.Url
                    });
                }
            }

            _context.SaveChanges();
        }

        public void UpdateTour(int id, CreateTourDto dto)
        {
            var tour = _repo.GetById(id);

            if (tour == null)
                throw new AppException(NotFoundCode, "Тур не знайдено");

            tour.Name = dto.Name.Trim();
            tour.Description = dto.Description;
            tour.Price = dto.Price;
            tour.DurationDays = dto.DurationDays;
            tour.AvailableFrom = dto.AvailableFrom;
            tour.AvailableTo = dto.AvailableTo;
            tour.TypeId = dto.TypeId;

            _repo.Update(tour);

            UpdateTourAssets(id, dto.Assets);
            _logger.LogInformation("Оновлено тур ID={TourId}", id);
        }

        private static TourDto MapToDto(Tour t)
        {
            return new TourDto
            {
                TourId = t.TourId,
                Name = t.Name,
                Description = t.Description,
                Price = t.Price,
                DurationDays = t.DurationDays,
                AvailableFrom = t.AvailableFrom,
                AvailableTo = t.AvailableTo,
                TypeId = t.TypeId,
                Assets = t.TourAssets != null
                    ? t.TourAssets.Select(a => new TourAssetDto
                    {
                        AssetType = a.AssetType,
                        Url = a.Url
                    }).ToList()
                    : new List<TourAssetDto>()
            };
        }

        private static void DeleteAssetFile(TourAsset asset)
        {
            var folder = asset.AssetType == VideoAssetType
                ? "videos"
                : "images";

            var fileName = Path.GetFileName(asset.Url);

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                folder,
                fileName
            );

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public List<TourDto> GetAll()
        {
            var tours = _repo.GetAll();
            return tours.Select(MapToDto).ToList();
        }



        public TourDto GetById(int id)
        {
            var tour = _repo.GetById(id);

            if (tour == null)
                throw new AppException(NotFoundCode, "Тур не знайдено");

            return MapToDto(tour);
        }



        public List<TourDto> Search(string query)
        {
            var tours = _repo.Search(query);
            return tours.Select(MapToDto).ToList();
        }


        public void AddAsset(int tourId, string url, string assetType)
        {
            var tour = _repo.GetById(tourId);

            if (tour == null)
                throw new AppException(NotFoundCode, "Тур не знайдено");

            _context.TourAssets.Add(new TourAsset
            {
                TourId = tourId,
                AssetType = assetType,
                Url = url
            });

            _context.SaveChanges();
        }

        public void DeleteTour(int id)
        {
            var tour = _repo.GetById(id);
            if (tour == null)
                throw new AppException(NotFoundCode, "Тур не знайдено");


            foreach (var asset in tour.TourAssets)
            {
                DeleteAssetFile(asset);
            }


            var bookings = _context.Bookings.Where(b => b.TourId == id).ToList();
            var bookingIds = bookings.Select(b => b.BookingId).ToList();

            _context.BookingPersons.RemoveRange(
                _context.BookingPersons.Where(p => bookingIds.Contains(p.BookingId))
            );

            _context.Bookings.RemoveRange(bookings);

            _context.TourAssets.RemoveRange(
                _context.TourAssets.Where(x => x.TourId == id)
            );

            _repo.Delete(tour);
            _logger.LogInformation("Видалено тур ID={TourId}", id);
        }


        public PagedResultDto<TourDto> Filter(
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            int? minDays,
            int? maxDays,
            int? typeId,
            int page = 1,
            int pageSize = 6)
        {
            var (tours, totalCount) = _repo.Filter(
                search,
                minPrice,
                maxPrice,
                minDays,
                maxDays,
                typeId,
                page,
                pageSize
            );

            return new PagedResultDto<TourDto>
            {
                Items = tours.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

    
}
}
