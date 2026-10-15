using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Praktikum542.Controllers;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.SmetanyukTests.E2ETests
{
    public class FavoriteE2ETests
    {
        private (FavoriteController Controller, PraktikumContext Db) CreateE2EStack(int credentialId, string dbName)
        {
            var options = new DbContextOptionsBuilder<PraktikumContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var dbContext = new PraktikumContext(options);
            var repo = new FavoriteRepository(dbContext);
            var service = new FavoriteService(repo);
            var controller = new FavoriteController(service);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, credentialId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "E2EAuthScheme");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            return (controller, dbContext);
        }

        [Fact]
        public void E2E_FullFavoriteLifecycle_AddGetDeleteWorkflow()
        {
            // Початкове налаштування бази даних та сутностей
            string dbName = Guid.NewGuid().ToString();
            int userId = 100;
            var (controller, db) = CreateE2EStack(userId, dbName);

            var tour = new Tour
            {
                TourId = 1,
                Name = "E2E Тур по Драгобрату",
                Description = "Активний відпочинок",
                Price = 5000,
                DurationDays = 5
            };
            db.Tours.Add(tour);
            db.SaveChanges();

            // Додаємо тур в обране через контролер
            var addResponse = controller.Add(new AddFavoriteDto { TourId = 1 });
            var addOkResult = Assert.IsType<OkObjectResult>(addResponse);
            Assert.Equal("Додано в обране", addOkResult.Value);

            // Запитуємо список обраного та перевіряємо вміст
            var getResponse = controller.GetMy();
            var getOkResult = Assert.IsType<OkObjectResult>(getResponse);
            var favoritesList = Assert.IsType<List<FavoriteDto>>(getOkResult.Value);

            Assert.Single(favoritesList);
            Assert.Equal("E2E Тур по Драгобрату", favoritesList[0].TourName);

            // Видаляємо тур з обраного
            var deleteResponse = controller.Delete(1);
            var deleteOkResult = Assert.IsType<OkObjectResult>(deleteResponse);
            Assert.Equal("Видалено з обраного", deleteOkResult.Value);

            // Повторно запитуємо список і переконуємося, що він порожній
            var finalGetResponse = controller.GetMy();
            var finalOkResult = Assert.IsType<OkObjectResult>(finalGetResponse);
            var finalFavoritesList = Assert.IsType<List<FavoriteDto>>(finalOkResult.Value);

            Assert.Empty(finalFavoritesList);
        }

        [Fact]
        public void E2E_MultiUserIsolation_UsersDoNotSeeOrModifyOthersFavorites()
        {
            // Налаштування спільної бази даних для кількох користувачів
            string dbName = Guid.NewGuid().ToString();
            int user1Id = 1;
            int user2Id = 2;

            var (user1Controller, db) = CreateE2EStack(user1Id, dbName);
            var (user2Controller, _) = CreateE2EStack(user2Id, dbName);

            var tour1 = new Tour { TourId = 10, Name = "Тур 1", Description = "Desc 1", Price = 1000, DurationDays = 2 };
            var tour2 = new Tour { TourId = 20, Name = "Тур 2", Description = "Desc 2", Price = 2000, DurationDays = 3 };
            db.Tours.AddRange(tour1, tour2);
            db.SaveChanges();

            // User 1 додає Тур 1, User 2 додає Тур 2
            user1Controller.Add(new AddFavoriteDto { TourId = 10 });
            user2Controller.Add(new AddFavoriteDto { TourId = 20 });

            // Перевірка списку User 1
            var res1 = Assert.IsType<OkObjectResult>(user1Controller.GetMy());
            var user1List = Assert.IsType<List<FavoriteDto>>(res1.Value);
            Assert.Single(user1List);
            Assert.Equal("Тур 1", user1List[0].TourName);

            // Перевірка списку User 2
            var res2 = Assert.IsType<OkObjectResult>(user2Controller.GetMy());
            var user2List = Assert.IsType<List<FavoriteDto>>(res2.Value);
            Assert.Single(user2List);
            Assert.Equal("Тур 2", user2List[0].TourName);

            // User 1 видаляє свій тур, це не повинно вплинути на User 2
            user1Controller.Delete(10);

            var res2After = Assert.IsType<OkObjectResult>(user2Controller.GetMy());
            var user2ListAfter = Assert.IsType<List<FavoriteDto>>(res2After.Value);
            Assert.Single(user2ListAfter);
            Assert.Equal("Тур 2", user2ListAfter[0].TourName);
        }
    }
}