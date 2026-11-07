using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
namespace Praktikum542.Controllers
{
    [Route("api/tours")]
    [ApiController]
    public class TourController : ControllerBase
    {
        private readonly TourService _service;
        private readonly PraktikumContext _context;
        private readonly IMemoryCache _cache;


        public TourController(
            TourService service,
            PraktikumContext context,
            IMemoryCache cache)
        {
            _service = service;
            _context = context;
            _cache = cache;
        }


        [Authorize(Roles = "manager")]
        [HttpPost]
        public IActionResult Create([FromBody] CreateTourDto dto)
        {
            var id = _service.CreateTour(dto);
            InvalidateTourCache();
            return Ok(new { tourId = id });
        }


        [HttpGet("types")]
        public IActionResult GetTypes()
        {
            var types = _cache.GetOrCreate("tours:types", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                return _context.TourTypes
                    .AsNoTracking()
                    .Select(t => new { t.TypeId, t.Name })
                    .ToList();
            });

            return Ok(types);
        }



        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] string? search,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int? minDays,
            [FromQuery] int? maxDays,
            [FromQuery] int? typeId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 6)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Некоректні параметри пагінації");

            if (minPrice < 0 || maxPrice < 0 ||
                (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice))
                return BadRequest("Некоректний діапазон цін");

            if (minDays < 1 || maxDays < 1 ||
                (minDays.HasValue && maxDays.HasValue && minDays > maxDays))
                return BadRequest("Некоректна тривалість туру");

            if (typeId.HasValue && typeId.Value < 1)
                return BadRequest("Некоректний тип туру");


            var version = _cache.GetOrCreate(
                "tours:version",
                entry => Guid.NewGuid().ToString("N")
            );

            var parameters = JsonSerializer.Serialize(new
            {
                search,
                minPrice,
                maxPrice,
                minDays,
                maxDays,
                typeId,
                page,
                pageSize
            });

            var cacheKey = $"tours:list:{version}:{parameters}";

            var result = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

                return _service.Filter(
                    search, minPrice, maxPrice,
                    minDays, maxDays, typeId,
                    page, pageSize
                );
            });

            return Ok(result);

        }


        [Authorize(Roles = "manager")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, CreateTourDto dto)
        {
            _service.UpdateTour(id, dto);
            InvalidateTourCache();
            return Ok("Tour updated");
        }

        [Authorize(Roles = "manager")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _service.DeleteTour(id);
            InvalidateTourCache();
            return Ok("Deleted");
        }

        [Authorize(Roles = "manager")]
        [HttpPost("{tourId}/images")]
        [RequestSizeLimit(500_000_000)]
        public async Task<IActionResult> UploadTourImage(int tourId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");



            if (tourId <= 0)
                return BadRequest("Некоректний ID туру");

            if (!_context.Tours.Any(t => t.TourId == tourId))
                return NotFound("Тур не знайдено");


            var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            var allowedVideoTypes = new[] { "video/mp4", "video/webm", "video/quicktime" };
            var allowedTypes = allowedImageTypes.Concat(allowedVideoTypes);

            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("Непідтримуваний формат файлу");

            var folder = file.ContentType.StartsWith("video") ? "videos" : "images";
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(dir, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"https://localhost:7041/{folder}/{fileName}";
            var assetType = file.ContentType.StartsWith("video") ? "video" : "image";

            _service.AddAsset(tourId, url, assetType);
            InvalidateTourCache();

            return Ok(new { url });
        }

        private void InvalidateTourCache()
        {
            _cache.Set(
                "tours:version",
                Guid.NewGuid().ToString("N")
            );

            _cache.Remove("tours:types");
        }

    }
}
