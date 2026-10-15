
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.KasapchukTests.E2ETests;

public class FavoritesAndPassengersE2ETests
{
    private sealed class E2ETestFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName =
            "KasapchukE2E_" + Guid.NewGuid().ToString("N");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<PraktikumContext>>();

                services.AddDbContext<PraktikumContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }

    private static HttpClient CreateAuthorizedClient(
        E2ETestFactory factory)
    {
        string token;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<PraktikumContext>();

            context.Database.EnsureCreated();

            var user = new Credential
            {
                CredentialId = 8501,
                Email = "kasapchuk.e2e@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                Role = "client",
                Status = "Active",
                CreatedAt = DateTime.Now,
                MustChangePassword = false
            };

            context.Credentials.Add(user);

            context.TourTypes.Add(new TourType
            {
                TypeId = 8501,
                Name = "E2E Nature Tours"
            });

            context.Tours.AddRange(
                new Tour
                {
                    TourId = 8501,
                    Name = "Carpathian Forest",
                    Description = "Forest hiking tour",
                    Price = 3500,
                    DurationDays = 4,
                    TypeId = 8501,
                    AvailableFrom = new DateOnly(2026, 10, 1),
                    AvailableTo = new DateOnly(2026, 12, 31)
                },
                new Tour
                {
                    TourId = 8502,
                    Name = "Mountain Lake",
                    Description = "Lake sightseeing tour",
                    Price = 5200,
                    DurationDays = 6,
                    TypeId = 8501,
                    AvailableFrom = new DateOnly(2026, 10, 1),
                    AvailableTo = new DateOnly(2026, 12, 31)
                });

            context.SaveChanges();

            var authService = scope.ServiceProvider
                .GetRequiredService<AuthentificationService>();

            token = authService.GenerateJwtToken(user);
        }

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private static async Task AddFavorite(
        HttpClient client,
        int tourId)
    {
        var response = await client.PostAsJsonAsync(
            "/api/favorites",
            new AddFavoriteDto { TourId = tourId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<List<FavoriteDto>> GetFavorites(
        HttpClient client)
    {
        var response = await client.GetAsync("/api/favorites");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var favorites = await response.Content
            .ReadFromJsonAsync<List<FavoriteDto>>();

        Assert.NotNull(favorites);
        return favorites!;
    }

    private static async Task<List<SavedPersonDto>> GetPassengers(
        HttpClient client)
    {
        var response = await client.GetAsync("/api/savedpersons");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var passengers = await response.Content
            .ReadFromJsonAsync<List<SavedPersonDto>>();

        Assert.NotNull(passengers);
        return passengers!;
    }

    // E2E 1: додавання туру та перевірка його у списку
    [Fact]
    public async Task User_ShouldAddTourAndSeeItsDetails_InFavorites()
    {
        using var factory = new E2ETestFactory();
        using var client = CreateAuthorizedClient(factory);

        await AddFavorite(client, 8501);

        var favorites = await GetFavorites(client);

        var favorite = Assert.Single(favorites);

        Assert.Equal(8501, favorite.TourId);
        Assert.Equal("Carpathian Forest", favorite.TourName);
        Assert.Equal(3500m, favorite.Price);
        Assert.Equal(4, favorite.DurationDays);
    }

    // E2E 2: додавання, видалення та перевірка порожнього списку
    [Fact]
    public async Task User_ShouldRemoveFavoriteAndSeeEmptyList_AfterDeletion()
    {
        using var factory = new E2ETestFactory();
        using var client = CreateAuthorizedClient(factory);

        await AddFavorite(client, 8501);

        var beforeDeletion = await GetFavorites(client);
        Assert.Single(beforeDeletion);

        var deleteResponse = await client.DeleteAsync(
            "/api/favorites/8501");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var afterDeletion = await GetFavorites(client);

        Assert.Empty(afterDeletion);
    }

    // E2E 3: додавання двох турів і видалення одного
    [Fact]
    public async Task User_ShouldKeepSecondFavorite_WhenFirstFavoriteIsRemoved()
    {
        using var factory = new E2ETestFactory();
        using var client = CreateAuthorizedClient(factory);

        await AddFavorite(client, 8501);
        await AddFavorite(client, 8502);

        var initialFavorites = await GetFavorites(client);

        Assert.Equal(2, initialFavorites.Count);

        var deleteResponse = await client.DeleteAsync(
            "/api/favorites/8501");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var remainingFavorites = await GetFavorites(client);

        var remaining = Assert.Single(remainingFavorites);

        Assert.Equal(8502, remaining.TourId);
        Assert.Equal("Mountain Lake", remaining.TourName);
    }

    // E2E 4: збереження пасажира та перевірка даних
    [Fact]
    public async Task User_ShouldSavePassengerAndSeeCorrectData_InSavedPersons()
    {
        using var factory = new E2ETestFactory();
        using var client = CreateAuthorizedClient(factory);

        var passenger = new SavedPersonDto
        {
            Name = "Andrii Melnyk",
            PassportData = "123456789",
            DateOfBirth = new DateOnly(2001, 5, 15),
            IsChild = false
        };

        var createResponse = await client.PostAsJsonAsync(
            "/api/savedpersons",
            passenger);

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var passengers = await GetPassengers(client);

        var saved = Assert.Single(passengers);

        Assert.True(saved.PersonId > 0);
        Assert.Equal("Andrii Melnyk", saved.Name);
        Assert.Equal("123456789", saved.PassportData);
        Assert.Equal(new DateOnly(2001, 5, 15), saved.DateOfBirth);
        Assert.False(saved.IsChild);
    }

    // E2E 5: збереження, видалення та перевірка пасажира
    [Fact]
    public async Task User_ShouldDeleteSavedPassenger_AndSeeEmptyList()
    {
        using var factory = new E2ETestFactory();
        using var client = CreateAuthorizedClient(factory);

        var passenger = new SavedPersonDto
        {
            Name = "Olena Bondarenko",
            PassportData = "987654321",
            DateOfBirth = new DateOnly(1999, 8, 20),
            IsChild = false
        };

        var createResponse = await client.PostAsJsonAsync(
            "/api/savedpersons",
            passenger);

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var passengers = await GetPassengers(client);
        var saved = Assert.Single(passengers);

        var deleteResponse = await client.DeleteAsync(
            $"/api/savedpersons/{saved.PersonId}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var afterDeletion = await GetPassengers(client);

        Assert.Empty(afterDeletion);
    }
}
