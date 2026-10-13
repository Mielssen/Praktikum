
using System;
using Microsoft.EntityFrameworkCore;
using Praktikum542.Exceptions;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.KasapchukTests.ExceptionTests;

public class FavoritesAndSavedPersonsExceptionTests
{
    private static PraktikumContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PraktikumContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PraktikumContext(options);
    }

    [Fact]
    public void AddFavorite_ShouldThrowNotFound_WhenTourDoesNotExist()
    {
        using var context = CreateContext();
        var service = new FavoriteService(new FavoriteRepository(context));

        var exception = Assert.Throws<AppException>(() =>
            service.Add(1, 999));

        Assert.Equal("NOT_FOUND", exception.Code);
        Assert.Equal("Тур не знайдено", exception.Message);
    }

    [Fact]
    public void AddFavorite_ShouldThrowAlreadyExists_WhenTourIsAlreadyFavorite()
    {
        using var context = CreateContext();

        context.Tours.Add(new Tour
        {
            TourId = 10,
            Name = "Mountain Trip"
        });

        context.Favorites.Add(new Favorite
        {
            CredentialId = 1,
            TourId = 10,
            CreatedAt = DateTime.Now
        });

        context.SaveChanges();

        var service = new FavoriteService(new FavoriteRepository(context));

        var exception = Assert.Throws<AppException>(() =>
            service.Add(1, 10));

        Assert.Equal("ALREADY_EXISTS", exception.Code);
        Assert.Equal("Тур вже в обраному", exception.Message);
    }

    [Fact]
    public void DeleteFavorite_ShouldThrowNotFound_WhenFavoriteDoesNotExist()
    {
        using var context = CreateContext();
        var service = new FavoriteService(new FavoriteRepository(context));

        var exception = Assert.Throws<AppException>(() =>
            service.Delete(1, 999));

        Assert.Equal("NOT_FOUND", exception.Code);
        Assert.Equal("Тур не в обраному", exception.Message);
    }

    [Fact]
    public void DeleteSavedPerson_ShouldThrowNotFound_WhenPersonDoesNotExist()
    {
        using var context = CreateContext();
        var service = new SavedPersonService(new SavedPersonRepository(context));

        var exception = Assert.Throws<AppException>(() =>
            service.Delete(1, 999));

        Assert.Equal("NOT_FOUND", exception.Code);
        Assert.Equal("Пресет не знайдено", exception.Message);
    }

    [Fact]
    public void DeleteSavedPerson_ShouldThrowForbidden_WhenPersonBelongsToAnotherUser()
    {
        using var context = CreateContext();

        context.SavedPersons.Add(new SavedPerson
        {
            PersonId = 5,
            CredentialId = 2,
            Name = "Ivan Petrenko",
            PassportData = "123456789",
            DateOfBirth = new DateOnly(2000, 5, 10),
            IsChild = false
        });

        context.SaveChanges();

        var service = new SavedPersonService(new SavedPersonRepository(context));

        var exception = Assert.Throws<AppException>(() =>
            service.Delete(1, 5));

        Assert.Equal("FORBIDDEN", exception.Code);
        Assert.Equal("Це не ваш пресет", exception.Message);
    }
}
