using System;
using System.Collections.Generic;
using Moq;
using Praktikum542.DTOs;
using Praktikum542.Exceptions;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.SmetanyukTests.MockingTests
{
    public class FavoriteServiceMockTests
    {
        private readonly Mock<FavoriteRepository> _mockRepo;
        private readonly FavoriteService _service;

        public FavoriteServiceMockTests()
        {
            _mockRepo = new Mock<FavoriteRepository>(new object[] { null! });
            _service = new FavoriteService(_mockRepo.Object);
        }

        [Fact]
        public void GetMy_WhenFavoritesExist_ReturnsMappedFavoriteDtoList()
        {
            int credentialId = 1;
            var favoritesList = new List<Favorite>
            {
                new Favorite
                {
                    FavoriteId = 10,
                    TourId = 101,
                    CredentialId = credentialId,
                    CreatedAt = DateTime.Now,
                    Tour = new Tour
                    {
                        TourId = 101,
                        Name = "Карпати Вікенд",
                        Description = "Чудовий тур",
                        Price = 2500m,
                        DurationDays = 3
                    }
                }
            };

            _mockRepo.Setup(r => r.GetByCredentialId(credentialId)).Returns(favoritesList);

            var result = _service.GetMy(credentialId);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(10, result[0].FavoriteId);
            Assert.Equal("Карпати Вікенд", result[0].TourName);
            _mockRepo.Verify(r => r.GetByCredentialId(credentialId), Times.Once);
        }

        [Fact]
        public void Add_WhenTourExistsAndNotAlreadyInFavorites_SuccessfullyCallsAdd()
        {
            int credentialId = 1;
            int tourId = 5;

            _mockRepo.Setup(r => r.TourExists(tourId)).Returns(true);
            _mockRepo.Setup(r => r.GetByCredentialAndTour(credentialId, tourId)).Returns((Favorite?)null);
            _mockRepo.Setup(r => r.Add(It.IsAny<Favorite>())).Verifiable();

            _service.Add(credentialId, tourId);

            _mockRepo.Verify(r => r.TourExists(tourId), Times.Once);
            _mockRepo.Verify(r => r.GetByCredentialAndTour(credentialId, tourId), Times.Once);
            _mockRepo.Verify(r => r.Add(It.Is<Favorite>(f => f.CredentialId == credentialId && f.TourId == tourId)), Times.Once);
        }

        [Fact]
        public void Add_WhenTourDoesNotExist_ThrowsAppExceptionAndNeverCallsAdd()
        {
            int credentialId = 1;
            int tourId = 999;

            _mockRepo.Setup(r => r.TourExists(tourId)).Returns(false);

            var exception = Assert.Throws<AppException>(() => _service.Add(credentialId, tourId));
            Assert.Equal("NOT_FOUND", exception.Code);
            Assert.Equal("Тур не знайдено", exception.Message);

            _mockRepo.Verify(r => r.TourExists(tourId), Times.Once);
            _mockRepo.Verify(r => r.Add(It.IsAny<Favorite>()), Times.Never);
        }

        [Fact]
        public void Add_WhenTourAlreadyInFavorites_ThrowsAppExceptionAndNeverCallsAdd()
        {
            int credentialId = 1;
            int tourId = 5;
            var existingFavorite = new Favorite { FavoriteId = 1, CredentialId = credentialId, TourId = tourId };

            _mockRepo.Setup(r => r.TourExists(tourId)).Returns(true);
            _mockRepo.Setup(r => r.GetByCredentialAndTour(credentialId, tourId)).Returns(existingFavorite);

            var exception = Assert.Throws<AppException>(() => _service.Add(credentialId, tourId));
            Assert.Equal("ALREADY_EXISTS", exception.Code);
            Assert.Equal("Тур вже в обраному", exception.Message);

            _mockRepo.Verify(r => r.Add(It.IsAny<Favorite>()), Times.Never);
        }

        [Fact]
        public void Delete_WhenFavoriteExists_SuccessfullyCallsDelete()
        {

            int credentialId = 1;
            int tourId = 5;
            var existingFavorite = new Favorite { FavoriteId = 10, CredentialId = credentialId, TourId = tourId };

            _mockRepo.Setup(r => r.GetByCredentialAndTour(credentialId, tourId)).Returns(existingFavorite);
            _mockRepo.Setup(r => r.Delete(existingFavorite)).Verifiable();

            _service.Delete(credentialId, tourId);

            _mockRepo.Verify(r => r.GetByCredentialAndTour(credentialId, tourId), Times.Once);
            _mockRepo.Verify(r => r.Delete(existingFavorite), Times.Once);
        }
    }
}