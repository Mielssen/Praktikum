using Microsoft.Extensions.Logging.Abstractions;
using Praktikum542.DTOs;
using Praktikum542.Exceptions;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.SmetanyukTests;

public class ExceptionTests
{
    // 1. Додавання в обране неіснуючого туру
    [Fact]
    public void FavoriteAdd_TourDoesNotExist_ThrowsNotFound()
    {
        using var context = TestHelper.CreateContext();
        var service = new FavoriteService(new FavoriteRepository(context));

        var ex = Assert.Throws<AppException>(() => service.Add(1, tourId: 999));

        Assert.Equal("NOT_FOUND", ex.Code);
        Assert.Equal("Тур не знайдено", ex.Message);
    }

    // 2. Повторне додавання того ж туру в обране
    [Fact]
    public void FavoriteAdd_TourAlreadyInFavorites_ThrowsAlreadyExists()
    {
        using var context = TestHelper.CreateContext();
        TestHelper.SeedTour(context);
        var service = new FavoriteService(new FavoriteRepository(context));
        service.Add(1, 1);

        var ex = Assert.Throws<AppException>(() => service.Add(1, 1));

        Assert.Equal("ALREADY_EXISTS", ex.Code);
        Assert.Single(context.Favorites); // дубліката в БД немає
    }

    // 3. Видалення чужого пресету
    [Fact]
    public void SavedPersonDelete_PresetBelongsToAnotherUser_ThrowsForbidden()
    {
        using var context = TestHelper.CreateContext();
        var preset = new SavedPerson
        {
            CredentialId = 1,
            Name = "Іван",
            PassportData = "123456",
            IsChild = false
        };
        context.SavedPersons.Add(preset);
        context.SaveChanges();

        var service = new SavedPersonService(new SavedPersonRepository(context));

        var ex = Assert.Throws<AppException>(() => service.Delete(credentialId: 2, personId: preset.PersonId));

        Assert.Equal("FORBIDDEN", ex.Code);
        Assert.Single(context.SavedPersons); // пресет лишився
    }

    // 4. Бронювання з датою початку раніше, ніж тур стає доступним
    [Fact]
    public void BookingCreate_StartDateBeforeAvailableFrom_ThrowsInvalidDate()
    {
        using var context = TestHelper.CreateContext();
        TestHelper.SeedTour(context); // доступний з 2030-01-01
        var service = new BookingService(
            new BookingRepository(context),
            context,
            NullLogger<BookingService>.Instance);

        var dto = new CreateBookingDto
        {
            TourId = 1,
            StartDate = new DateOnly(2029, 12, 1),
            NumberOfAdults = 1,
            NumberOfChildren = 0,
            Persons = new List<BookingPersonDto>()
        };

        var ex = Assert.Throws<AppException>(() => service.Create(1, dto));

        Assert.Equal("INVALID_DATE", ex.Code);
        Assert.Empty(context.Bookings);
    }

    // 5. Логін з некоректним email (валідація спрацьовує до звернення до БД)
    [Fact]
    public void Login_InvalidEmailFormat_ThrowsInvalidEmail()
    {
        var service = new AuthentificationService(
            null!, null!, null!, null!,
            NullLogger<AuthentificationService>.Instance);

        var ex = Assert.Throws<AppException>(() =>
            service.Login(new LoginDto { Email = "not-an-email", Password = "123456" }));

        Assert.Equal("INVALID_EMAIL", ex.Code);
    }
}