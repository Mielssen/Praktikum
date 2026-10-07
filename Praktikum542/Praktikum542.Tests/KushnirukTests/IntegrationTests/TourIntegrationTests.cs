using Microsoft.Extensions.DependencyInjection;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Services;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Praktikum542.Tests.KushnirukTests.IntegrationTests;

public class TourIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public TourIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTypes_ShouldReturnTourTypes_FromDatabase()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<PraktikumContext>();

            context.TourTypes.AddRange(
                new TourType
                {
                    TypeId = 1,
                    Name = "Екскурсійний"
                },
                new TourType
                {
                    TypeId = 2,
                    Name = "Пляжний"
                }
            );

            context.SaveChanges();
        }

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tours/types");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<List<TourTypeResponse>>();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Contains(
            result,
            x => x.TypeId == 1 &&
                 x.Name == "Екскурсійний");

        Assert.Contains(
            result,
            x => x.TypeId == 2 &&
                 x.Name == "Пляжний");
    }

    private class TourTypeResponse
    {
        public int TypeId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    private class PagedTourResponse
    {
        public List<TourResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    private class TourResponse
    {
        public int TourId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }



    [Fact]
    public async Task GetTours_ShouldReturnOnlyToursWithinPriceRange()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<PraktikumContext>();

            var tourType = new TourType
            {
                TypeId = 100,
                Name = "Тестовий тип"
            };

            context.TourTypes.Add(tourType);

            context.Tours.AddRange(
                new Tour
                {
                    TourId = 101,
                    Name = "Дешевий тур",
                    Description = "Test",
                    Price = 500,
                    DurationDays = 3,
                    AvailableFrom = new DateOnly(2026, 10, 1),
                    AvailableTo = new DateOnly(2026, 12, 31),
                    TypeId = 100
                },
                new Tour
                {
                    TourId = 102,
                    Name = "Середній тур",
                    Description = "Test",
                    Price = 3000,
                    DurationDays = 5,
                    AvailableFrom = new DateOnly(2026, 10, 1),
                    AvailableTo = new DateOnly(2026, 12, 31),
                    TypeId = 100
                },
                new Tour
                {
                    TourId = 103,
                    Name = "Дорогий тур",
                    Description = "Test",
                    Price = 8000,
                    DurationDays = 7,
                    AvailableFrom = new DateOnly(2026, 10, 1),
                    AvailableTo = new DateOnly(2026, 12, 31),
                    TypeId = 100
                }
            );

            context.SaveChanges();
        }

        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            "/api/tours?minPrice=1000&maxPrice=5000&page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<PagedTourResponse>();

        Assert.NotNull(result);

        Assert.Contains(
            result.Items,
            x => x.Name == "Середній тур" &&
                 x.Price == 3000);

        Assert.DoesNotContain(
            result.Items,
            x => x.Name == "Дешевий тур");

        Assert.DoesNotContain(
            result.Items,
            x => x.Name == "Дорогий тур");
    }
    [Fact]
    public async Task CreateTour_ShouldPersistTour_WhenManagerIsAuthorized()
    {
        string token;

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<PraktikumContext>();

            var tourType = new TourType
            {
                TypeId = 200,
                Name = "Активний відпочинок"
            };

            var manager = new Credential
            {
                CredentialId = 400,
                Email = "manager.integration@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123"),
                Role = "manager",
                Status = "Active",
                CreatedAt = DateTime.Now,
                MustChangePassword = false
            };

            context.TourTypes.Add(tourType);
            context.Credentials.Add(manager);
            context.SaveChanges();

            var authService = scope.ServiceProvider
                .GetRequiredService<AuthentificationService>();

            token = authService.GenerateJwtToken(manager);
        }

        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var dto = new CreateTourDto
        {
            Name = "Карпатська пригода",
            Description = "Інтеграційний тест створення туру",
            Price = 4500,
            DurationDays = 5,
            AvailableFrom = new DateOnly(2026, 11, 1),
            AvailableTo = new DateOnly(2026, 12, 31),
            TypeId = 200
        };

        var response = await client.PostAsJsonAsync(
            "/api/tours",
            dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<PraktikumContext>();

            var createdTour = context.Tours
                .FirstOrDefault(t => t.Name == "Карпатська пригода");

            Assert.NotNull(createdTour);
            Assert.Equal("Карпатська пригода", createdTour.Name);
            Assert.Equal("Інтеграційний тест створення туру", createdTour.Description);
            Assert.Equal(4500m, createdTour.Price);
            Assert.Equal(5, createdTour.DurationDays);
            Assert.Equal(new DateOnly(2026, 11, 1), createdTour.AvailableFrom);
            Assert.Equal(new DateOnly(2026, 12, 31), createdTour.AvailableTo);
            Assert.Equal(200, createdTour.TypeId);
        }
    }
}