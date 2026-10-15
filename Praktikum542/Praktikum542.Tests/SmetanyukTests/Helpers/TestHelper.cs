using Microsoft.EntityFrameworkCore;
using Praktikum542.Models;

namespace Praktikum542.Tests.SmetanyukTests;

public static class TestHelper
{
    /// <summary>Створює ізольовану in-memory БД для кожного тесту.</summary>
    public static PraktikumContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PraktikumContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PraktikumContext(options);
    }

    /// <summary>Додає в БД тур (і тип туру) та повертає його.</summary>
    public static Tour SeedTour(
        PraktikumContext context,
        int tourId = 1,
        decimal price = 1000m,
        int durationDays = 7,
        int typeId = 1,
        string name = "Тестовий тур")
    {
        if (!context.TourTypes.Any(t => t.TypeId == typeId))
            context.TourTypes.Add(new TourType { TypeId = typeId, Name = "Тип " + typeId });

        var tour = new Tour
        {
            TourId = tourId,
            Name = name,
            Description = "Опис",
            Price = price,
            DurationDays = durationDays,
            AvailableFrom = new DateOnly(2030, 1, 1),
            AvailableTo = new DateOnly(2030, 12, 31),
            TypeId = typeId
        };

        context.Tours.Add(tour);
        context.SaveChanges();
        return tour;
    }

    /// <summary>Додає бронювання напряму в БД (для тестів скасування).</summary>
    public static Booking SeedBooking(
        PraktikumContext context,
        int credentialId,
        int tourId,
        DateOnly startDate,
        decimal totalPrice = 2000m,
        string status = "pending")
    {
        var booking = new Booking
        {
            CredentialId = credentialId,
            TourId = tourId,
            StartDate = startDate,
            BookingDate = DateTime.Now,
            Status = status,
            NumberOfPeople = 2,
            TotalPrice = totalPrice
        };

        context.Bookings.Add(booking);
        context.SaveChanges();
        return booking;
    }
}