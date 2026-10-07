using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Services;
using Praktikum542.Tests.KushnirukTests.IntegrationTests;
using Xunit;

namespace Praktikum542.Tests.KushnirukTests.E2ETests;

public class BookingE2ETests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public BookingE2ETests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task User_ShouldCreateBookingAndSeeItInMyBookings()
    {
        string token;

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<PraktikumContext>();

            var user = new Credential
            {
                CredentialId = 700,
                Email = "booking.e2e@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                Role = "client",
                Status = "Active",
                CreatedAt = DateTime.Now,
                MustChangePassword = false
            };

            var tourType = new TourType
            {
                TypeId = 300,
                Name = "E2E Tour Type"
            };

            var tour = new Tour
            {
                TourId = 700,
                Name = "E2E Booking Tour",
                Description = "Tour for E2E booking test",
                Price = 4000,
                DurationDays = 5,
                AvailableFrom = new DateOnly(2026, 10, 1),
                AvailableTo = new DateOnly(2026, 12, 31),
                TypeId = 300
            };

            context.Credentials.Add(user);
            context.TourTypes.Add(tourType);
            context.Tours.Add(tour);

            context.SaveChanges();

            var authService = scope.ServiceProvider
                .GetRequiredService<AuthentificationService>();

            token = authService.GenerateJwtToken(user);
        }

        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var bookingDto = new CreateBookingDto
        {
            TourId = 700,
            StartDate = new DateOnly(2026, 11, 10),
            NumberOfAdults = 1,
            NumberOfChildren = 0,
            Comment = "E2E booking test",
            Persons = new List<BookingPersonDto>
            {
                new BookingPersonDto
                {
                    Name = "Test User",
                    PassportData = "123456789",
                    DateOfBirth = new DateOnly(2000, 1, 1),
                    IsChild = false
                }
            }
        };

        var createResponse = await client.PostAsJsonAsync(
            "/api/bookings",
            bookingDto);

        Assert.Equal(
            HttpStatusCode.OK,
            createResponse.StatusCode);

        var createContent = await createResponse.Content
            .ReadAsStringAsync();

        Assert.Contains(
            "Бронювання створено",
            createContent);

        var myBookingsResponse = await client.GetAsync(
            "/api/bookings/my");

        Assert.Equal(
            HttpStatusCode.OK,
            myBookingsResponse.StatusCode);

        var bookings = await myBookingsResponse.Content
            .ReadFromJsonAsync<List<BookingResponse>>();

        Assert.NotNull(bookings);

        Assert.Contains(
            bookings,
            booking =>
                booking.TourId == 700 &&
                booking.TourName == "E2E Booking Tour" &&
                booking.Status == "pending" &&
                booking.NumberOfPeople == 1 &&
                booking.TotalPrice == 4000m);
    }

    private class BookingResponse
    {
        public int BookingId { get; set; }
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int NumberOfPeople { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Comment { get; set; }
    }
}