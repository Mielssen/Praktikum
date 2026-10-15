using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Praktikum542.Controllers;
using Praktikum542.DTOs;
using Praktikum542.Exceptions;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.SmetanyukTests.RestApiTests
{
    public class FavoriteControllerRestApiTests
    {
        private readonly Mock<FavoriteRepository> _mockRepo;
        private readonly FavoriteService _service;

        public FavoriteControllerRestApiTests()
        {
            _mockRepo = new Mock<FavoriteRepository>(new object[] { null! });
            _service = new FavoriteService(_mockRepo.Object);
        }

        private FavoriteController CreateControllerWithUser(int credentialId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, credentialId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            return new FavoriteController(_service)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };
        }

        [Fact]
        public void GetMy_AuthorizedUser_ReturnsOkWithFavoritesList()
        {
            int credentialId = 1;
            var controller = CreateControllerWithUser(credentialId);

            var favorites = new List<Favorite>
            {
                new Favorite
                {
                    FavoriteId = 10,
                    TourId = 101,
                    CredentialId = credentialId,
                    Tour = new Tour { Name = "Закарпаття", Description = "Релакс", Price = 3500, DurationDays = 5 }
                }
            };

            _mockRepo.Setup(r => r.GetByCredentialId(credentialId)).Returns(favorites);

            var result = controller.GetMy();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtoList = Assert.IsType<List<FavoriteDto>>(okResult.Value);
            Assert.Single(dtoList);
            Assert.Equal("Закарпаття", dtoList[0].TourName);
        }

        [Fact]
        public void GetMy_UserWithNoFavorites_ReturnsOkWithEmptyList()
        {
            int credentialId = 2;
            var controller = CreateControllerWithUser(credentialId);

            _mockRepo.Setup(r => r.GetByCredentialId(credentialId)).Returns(new List<Favorite>());

            var result = controller.GetMy();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtoList = Assert.IsType<List<FavoriteDto>>(okResult.Value);
            Assert.Empty(dtoList);
        }

        [Fact]
        public void Add_ValidDto_ReturnsOkResponseMessage()
        {
            int credentialId = 1;
            int tourId = 5;
            var controller = CreateControllerWithUser(credentialId);
            var dto = new AddFavoriteDto { TourId = tourId };

            _mockRepo.Setup(r => r.TourExists(tourId)).Returns(true);
            _mockRepo.Setup(r => r.GetByCredentialAndTour(credentialId, tourId)).Returns((Favorite?)null);

            var result = controller.Add(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Додано в обране", okResult.Value);
            _mockRepo.Verify(r => r.Add(It.Is<Favorite>(f => f.CredentialId == credentialId && f.TourId == tourId)), Times.Once);
        }

        [Fact]
        public void Delete_ExistingTourId_ReturnsOkResponseMessage()
        {
            int credentialId = 1;
            int tourId = 5;
            var controller = CreateControllerWithUser(credentialId);
            var favorite = new Favorite { FavoriteId = 1, CredentialId = credentialId, TourId = tourId };

            _mockRepo.Setup(r => r.GetByCredentialAndTour(credentialId, tourId)).Returns(favorite);

            var result = controller.Delete(tourId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Видалено з обраного", okResult.Value);
            _mockRepo.Verify(r => r.Delete(favorite), Times.Once);
        }

        [Fact]
        public void Add_DuplicateTour_ThrowsAppExceptionFromService()
        {
            int credentialId = 1;
            int tourId = 5;
            var controller = CreateControllerWithUser(credentialId);
            var dto = new AddFavoriteDto { TourId = tourId };
            var existingFavorite = new Favorite { FavoriteId = 1, CredentialId = credentialId, TourId = tourId };

            _mockRepo.Setup(r => r.TourExists(tourId)).Returns(true);
            _mockRepo.Setup(r => r.GetByCredentialAndTour(credentialId, tourId)).Returns(existingFavorite);

            var ex = Assert.Throws<AppException>(() => controller.Add(dto));
            Assert.Equal("ALREADY_EXISTS", ex.Code);
        }
    }
}