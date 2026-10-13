
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Praktikum542.Models;
using Xunit;

namespace Praktikum542.Tests.KasapchukTests.IntegrationTests;

public class ReviewIntegrationTests
{
    private sealed class ReviewTestFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName =
            "ReviewTests_" + Guid.NewGuid();

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

    private static async Task SeedReviews(
        ReviewTestFactory factory,
        params (int tourId, int rating, DateTime createdAt)[] reviews)
    {
        using var scope = factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<PraktikumContext>();

        context.TourTypes.Add(new TourType
        {
            TypeId = 1,
            Name = "Adventure"
        });

        context.Tours.AddRange(
            new Tour
            {
                TourId = 1,
                Name = "Forest Trip",
                Description = "Forest vacation",
                Price = 2500,
                DurationDays = 3,
                TypeId = 1,
                AvailableFrom = new DateOnly(2026, 1, 1),
                AvailableTo = new DateOnly(2026, 12, 31)
            },
            new Tour
            {
                TourId = 2,
                Name = "Sea Trip",
                Description = "Sea vacation",
                Price = 4500,
                DurationDays = 5,
                TypeId = 1,
                AvailableFrom = new DateOnly(2026, 1, 1),
                AvailableTo = new DateOnly(2026, 12, 31)
            });

        context.Credentials.Add(new Credential
        {
            CredentialId = 1,
            Email = "reviewer@example.com",
            PasswordHash = "test-hash",
            Role = "user",
            Status = "active",
            CreatedAt = DateTime.Now
        });

        await context.SaveChangesAsync();

        var bookingId = 1;

        foreach (var item in reviews)
        {
            context.Bookings.Add(new Booking
            {
                BookingId = bookingId,
                CredentialId = 1,
                TourId = item.tourId,
                StartDate = new DateOnly(2026, 1, 10),
                Status = "completed"
            });

            context.Reviews.Add(new Review
            {
                ReviewId = bookingId,
                BookingId = bookingId,
                CredentialId = 1,
                TourId = item.tourId,
                Rating = (sbyte)item.rating,
                Comment = "Review " + bookingId,
                CreatedAt = item.createdAt
            });

            bookingId++;
        }

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByTour_ShouldReturnEmptyList_WhenTourHasNoReviews()
    {
        using var factory = new ReviewTestFactory();
        await SeedReviews(factory);

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/reviews/tour/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reviews = await response.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        Assert.Equal(
            System.Text.Json.JsonValueKind.Array,
            reviews.ValueKind);

        Assert.Equal(0, reviews.GetArrayLength());
    }

    [Fact]
    public async Task GetByTour_ShouldReturnOnlyMatchingReviews_WhenMultipleToursExist()
    {
        using var factory = new ReviewTestFactory();

        await SeedReviews(factory,
            (1, 5, new DateTime(2026, 2, 1)),
            (1, 4, new DateTime(2026, 2, 2)),
            (2, 2, new DateTime(2026, 2, 3)));

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/reviews/tour/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reviews = await response.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        Assert.Equal(2, reviews.GetArrayLength());

        foreach (var review in reviews.EnumerateArray())
        {
            Assert.Equal(
                1,
                review.GetProperty("tourId").GetInt32());
        }
    }

    [Fact]
    public async Task GetByTour_ShouldReturnNewestReviewFirst_WhenReviewsHaveDifferentDates()
    {
        using var factory = new ReviewTestFactory();

        await SeedReviews(factory,
            (1, 3, new DateTime(2026, 1, 10)),
            (1, 5, new DateTime(2026, 3, 10)),
            (1, 4, new DateTime(2026, 2, 10)));

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/reviews/tour/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reviews = await response.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        var ratings = reviews.EnumerateArray()
            .Select(r => r.GetProperty("rating").GetInt32())
            .ToArray();

        Assert.Equal(new[] { 5, 4, 3 }, ratings);
    }

    [Fact]
    public async Task GetAverageRating_ShouldReturnZero_WhenTourHasNoReviews()
    {
        using var factory = new ReviewTestFactory();
        await SeedReviews(factory);

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/reviews/tour/1/average");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        Assert.Equal(
            1,
            result.GetProperty("tourId").GetInt32());

        Assert.Equal(
            0,
            result.GetProperty("averageRating").GetDouble());
    }

    [Fact]
    public async Task GetAverageRating_ShouldRoundToOneDecimal_WhenReviewsHaveDifferentRatings()
    {
        using var factory = new ReviewTestFactory();

        await SeedReviews(factory,
            (1, 5, new DateTime(2026, 2, 1)),
            (1, 4, new DateTime(2026, 2, 2)),
            (1, 2, new DateTime(2026, 2, 3)));

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/reviews/tour/1/average");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        Assert.Equal(
            3.7,
            result.GetProperty("averageRating").GetDouble(),
            1);
    }
}
