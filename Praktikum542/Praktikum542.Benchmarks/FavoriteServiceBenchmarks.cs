using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Praktikum542.Exceptions;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;

namespace Praktikum542.Benchmarks
{
    /// <summary>
    /// Бенчмарки FavoriteService (сервіс + репозиторій + EF Core) на InMemory-БД.
    /// Кожна операція виконується у власному DbContext, як окремий HTTP-запит (Scoped).
    /// FavoritesCount — розмір списку обраного користувача: мале / середнє / велике навантаження.
    /// </summary>
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    [CsvExporter]
    public class FavoriteServiceBenchmarks
    {
        private const int UserId = 1;
        private const int OtherUserId = 2;

        private readonly InMemoryDatabaseRoot _root = new();
        private DbContextOptions<PraktikumContext> _options = null!;
        private int _freeTourId;

        [Params(10, 100, 1000)]
        public int FavoritesCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<PraktikumContext>()
                .UseInMemoryDatabase($"favorites-bench-{Guid.NewGuid()}", _root)
                .Options;

            using var ctx = new PraktikumContext(_options);

            // FavoritesCount турів у обраному + 1 вільний тур для Add/Delete
            var tours = Enumerable.Range(1, FavoritesCount + 1)
                .Select(i => new Tour
                {
                    TourId = i,
                    Name = $"Tour {i}",
                    Description = "Опис туру для бенчмарку",
                    Price = 1000 + i,
                    DurationDays = 3 + i % 10,
                    TypeId = 1
                })
                .ToList();
            ctx.Tours.AddRange(tours);

            // Обране основного користувача + такий самий обсяг "чужих" записів,
            // щоб фільтр по CredentialId працював не на порожньому шумі.
            for (var i = 1; i <= FavoritesCount; i++)
            {
                ctx.Favorites.Add(new Favorite { CredentialId = UserId, TourId = i, CreatedAt = DateTime.Now });
                ctx.Favorites.Add(new Favorite { CredentialId = OtherUserId, TourId = i, CreatedAt = DateTime.Now });
            }

            ctx.SaveChanges();
            _freeTourId = FavoritesCount + 1;
        }

        private FavoriteService CreateService(PraktikumContext ctx)
            => new(new FavoriteRepository(ctx));

        [Benchmark(Description = "GetMy (список обраного)")]
        public int GetMy()
        {
            using var ctx = new PraktikumContext(_options);
            return CreateService(ctx).GetMy(UserId).Count;
        }

        [Benchmark(Description = "Add + Delete (додати і видалити тур)")]
        public void AddThenDelete()
        {
            using (var ctx = new PraktikumContext(_options))
                CreateService(ctx).Add(UserId, _freeTourId);

            using (var ctx = new PraktikumContext(_options))
                CreateService(ctx).Delete(UserId, _freeTourId);
        }

        [Benchmark(Description = "Add дубліката (AppException)")]
        public bool AddDuplicate()
        {
            using var ctx = new PraktikumContext(_options);
            try
            {
                CreateService(ctx).Add(UserId, 1);
                return false;
            }
            catch (AppException)
            {
                return true;
            }
        }
    }
}
