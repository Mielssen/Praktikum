using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Praktikum542.Exceptions;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.SmetanyukTests.IntegrationTests
{
    public class FavoriteServiceIntegrationTests
    {
        private PraktikumContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<PraktikumContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PraktikumContext(options);
        }

        [Fact]
        public void GetMy_Integration_ReturnsUserSpecificFavoritesWithTourDetails()
        {
            using var context = GetInMemoryContext();
            var tour = new Tour { TourId = 1, Name = "Карпати", Description = "Гори", Price = 3000, DurationDays = 4 };
            var favorite = new Favorite { FavoriteId = 10, CredentialId = 1, TourId = 1, Tour = tour, CreatedAt = DateTime.Now };

            context.Tours.Add(tour);
            context.Favorites.Add(favorite);
            context.SaveChanges();

            var repo = new FavoriteRepository(context);
            var service = new FavoriteService(repo);

            var result = service.GetMy(1);

            Assert.Single(result);
            Assert.Equal("Карпати", result[0].TourName);
            Assert.Equal(3000, result[0].Price);
        }

        [Fact]
        public void Add_Integration_ValidTour_PersistsToDatabase()
        {
            using var context = GetInMemoryContext();
            var tour = new Tour { TourId = 2, Name = "Одеса", Description = "Море", Price = 2000, DurationDays = 3 };
            context.Tours.Add(tour);
            context.SaveChanges();

            var repo = new FavoriteRepository(context);
            var service = new FavoriteService(repo);

            service.Add(credentialId: 5, tourId: 2);

            var savedFavorite = context.Favorites.FirstOrDefault(f => f.CredentialId == 5 && f.TourId == 2);
            Assert.NotNull(savedFavorite);
            Assert.Equal(2, savedFavorite.TourId);
        }

        [Fact]
        public void Add_Integration_NonExistingTour_ThrowsAppExceptionAndDoesNotPersist()
        {
            using var context = GetInMemoryContext();
            var repo = new FavoriteRepository(context);
            var service = new FavoriteService(repo);

            var ex = Assert.Throws<AppException>(() => service.Add(credentialId: 1, tourId: 999));
            Assert.Equal("NOT_FOUND", ex.Code);
            Assert.Equal(0, context.Favorites.Count());
        }

        [Fact]
        public void Delete_Integration_ExistingFavorite_RemovesFromDatabase()
        {
            using var context = GetInMemoryContext();
            var favorite = new Favorite { FavoriteId = 1, CredentialId = 3, TourId = 10 };
            context.Favorites.Add(favorite);
            context.SaveChanges();

            var repo = new FavoriteRepository(context);
            var service = new FavoriteService(repo);

            service.Delete(credentialId: 3, tourId: 10);

            var deletedFavorite = context.Favorites.FirstOrDefault(f => f.CredentialId == 3 && f.TourId == 10);
            Assert.Null(deletedFavorite);
        }

        [Fact]
        public void Delete_Integration_NonExistingFavorite_ThrowsAppException()
        {
            using var context = GetInMemoryContext();
            var repo = new FavoriteRepository(context);
            var service = new FavoriteService(repo);

            var ex = Assert.Throws<AppException>(() => service.Delete(credentialId: 1, tourId: 55));
            Assert.Equal("NOT_FOUND", ex.Code);
        }
    }
}