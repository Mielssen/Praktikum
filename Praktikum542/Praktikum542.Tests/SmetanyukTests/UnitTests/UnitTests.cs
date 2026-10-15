using Microsoft.Extensions.Logging.Abstractions;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.SmetanyukTests;

public class UnitTests
{
    private static BookingService CreateBookingService(PraktikumContext context)
    {
        return new BookingService(
            new BookingRepository(context),
            context,
            NullLogger<BookingService>.Instance);
    }

    private static BookingPersonDto Person(string name, bool isChild) => new()
    {
        Name = name,
        PassportData = "123456",
        DateOfBirth = new DateOnly(1990, 1, 1),
        IsChild = isChild
    };

    // 1. Розрахунок ціни: дорослі за повну ціну, діти зі знижкою 50%
    [Fact]
    public void Create_TwoAdultsAndOneChild_CalculatesPriceWithChildDiscount()
    {
        using var context = TestHelper.CreateContext();
        TestHelper.SeedTour(context, price: 1000m);
        var service = CreateBookingService(context);

        service.Create(1, new CreateBookingDto
        {
            TourId = 1,
            StartDate = new DateOnly(2030, 6, 1),
            NumberOfAdults = 2,
            NumberOfChildren = 1,
            Persons = new List<BookingPersonDto>
            {
                Person("Іван", false),
                Person("Марія", false),
                Person("Олег", true)
            }
        });

        var booking = Assert.Single(context.Bookings);
        Assert.Equal(2500m, booking.TotalPrice); // 2*1000 + 1*1000*0.5
        Assert.Equal(3, booking.NumberOfPeople);
    }

    // 2. Нове бронювання отримує статус pending і зберігає учасників
    [Fact]
    public void Create_ValidData_SetsPendingStatusAndSavesPersons()
    {
        using var context = TestHelper.CreateContext();
        TestHelper.SeedTour(context);
        var service = CreateBookingService(context);

        service.Create(5, new CreateBookingDto
        {
            TourId = 1,
            StartDate = new DateOnly(2030, 6, 1),
            NumberOfAdults = 2,
            NumberOfChildren = 0,
            Persons = new List<BookingPersonDto>
            {
                Person("Іван", false),
                Person("Марія", false)
            }
        });

        var booking = Assert.Single(context.Bookings);
        Assert.Equal("pending", booking.Status);
        Assert.Equal(5, booking.CredentialId);
        Assert.Equal(2, context.BookingPersons.Count());
    }

    // 3. Скасування більш ніж за 3 дні до початку: без штрафу
    [Fact]
    public void Cancel_MoreThanThreeDaysBeforeStart_NoPenalty()
    {
        using var context = TestHelper.CreateContext();
        TestHelper.SeedTour(context);
        var start = DateOnly.FromDateTime(DateTime.Now).AddDays(10);
        var booking = TestHelper.SeedBooking(context, credentialId: 1, tourId: 1, start);
        var service = CreateBookingService(context);

        var penalty = service.Cancel(1, booking.BookingId);

        Assert.Equal(0m, penalty);
        Assert.Equal("cancelled", context.Bookings.Single().Status);
        Assert.Empty(context.Penalties);
    }

    // 4. Скасування менш ніж за 3 дні до початку: штраф 30% від суми
    [Fact]
    public void Cancel_LessThanThreeDaysBeforeStart_ChargesThirtyPercentPenalty()
    {
        using var context = TestHelper.CreateContext();
        TestHelper.SeedTour(context);
        var start = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        var booking = TestHelper.SeedBooking(context, 1, 1, start, totalPrice: 2000m);
        var service = CreateBookingService(context);

        var penalty = service.Cancel(1, booking.BookingId);

        Assert.Equal(600m, penalty); // 2000 * 0.30
        var saved = Assert.Single(context.Penalties);
        Assert.Equal(600m, saved.Amount);
        Assert.False(saved.IsPaid);
    }

    // 5. Отримання всіх турів повертає всі записи з правильними даними
    [Fact]
    public void GetAll_ReturnsAllToursWithCorrectPrices()
    {
        using var context = TestHelper.CreateContext();
        TestHelper.SeedTour(context, tourId: 1, price: 500m, name: "Дешевий");
        TestHelper.SeedTour(context, tourId: 2, price: 1000m, name: "Середній");
        TestHelper.SeedTour(context, tourId: 3, price: 9000m, name: "Дорогий");

        var service = new TourService(
            new TourRepository(context),
            context,
            NullLogger<TourService>.Instance);

        var result = service.GetAll();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, t => t.Name == "Середній" && t.Price == 1000m);
    }
}