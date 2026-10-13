
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Praktikum542.DTOs;
using Praktikum542.Exceptions;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.KasapchukTests.MockingTests;

public class TourLoggingMockingTests
{
    private static PraktikumContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PraktikumContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PraktikumContext(options);
    }

    private static TourService CreateService(
        PraktikumContext context,
        Mock<ILogger<TourService>> logger)
    {
        return new TourService(
            new TourRepository(context),
            context,
            logger.Object);
    }

    private static void AddTourType(PraktikumContext context)
    {
        context.TourTypes.Add(new TourType
        {
            TypeId = 1,
            Name = "Adventure"
        });

        context.SaveChanges();
    }

    private static Tour AddTour(PraktikumContext context)
    {
        var tour = new Tour
        {
            Name = "Mountain Tour",
            Description = "Mountain vacation",
            Price = 4500,
            DurationDays = 5,
            AvailableFrom = new DateOnly(2026, 10, 1),
            AvailableTo = new DateOnly(2026, 12, 31),
            TypeId = 1
        };

        context.Tours.Add(tour);
        context.SaveChanges();

        return tour;
    }

    private static CreateTourDto CreateDto(string name = "Forest Tour")
    {
        return new CreateTourDto
        {
            Name = name,
            Description = "Nature vacation",
            Price = 3200,
            DurationDays = 4,
            AvailableFrom = new DateOnly(2026, 10, 1),
            AvailableTo = new DateOnly(2026, 12, 31),
            TypeId = 1
        };
    }

    private static void VerifyLog(
        Mock<ILogger<TourService>> logger,
        string message,
        Times times)
    {
        logger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, type) =>
                state.ToString()!.Contains(message)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), times);
    }

    [Fact]
    public void CreateTour_ShouldWriteInformationLog_WhenTourIsCreated()
    {
        using var context = CreateContext();
        AddTourType(context);

        var logger = new Mock<ILogger<TourService>>();
        var service = CreateService(context, logger);

        service.CreateTour(CreateDto());

        VerifyLog(logger, "Створено тур", Times.Once());
    }

    [Fact]
    public void CreateTour_ShouldLogTourName_WhenCreationSucceeds()
    {
        using var context = CreateContext();
        AddTourType(context);

        var logger = new Mock<ILogger<TourService>>();
        var service = CreateService(context, logger);

        service.CreateTour(CreateDto("River Adventure"));

        VerifyLog(logger, "River Adventure", Times.Once());
    }

    [Fact]
    public void CreateTour_ShouldNotWriteInformationLog_WhenTourTypeDoesNotExist()
    {
        using var context = CreateContext();

        var logger = new Mock<ILogger<TourService>>();
        var service = CreateService(context, logger);

        Assert.Throws<AppException>(() =>
            service.CreateTour(CreateDto()));

        logger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Never());
    }

    [Fact]
    public void UpdateTour_ShouldWriteInformationLog_WhenTourIsUpdated()
    {
        using var context = CreateContext();
        AddTourType(context);
        var tour = AddTour(context);

        var logger = new Mock<ILogger<TourService>>();
        var service = CreateService(context, logger);

        service.UpdateTour(tour.TourId, CreateDto("Updated Tour"));

        VerifyLog(logger, "Оновлено тур", Times.Once());
    }

    [Fact]
    public void DeleteTour_ShouldNotWriteInformationLog_WhenTourDoesNotExist()
    {
        using var context = CreateContext();

        var logger = new Mock<ILogger<TourService>>();
        var service = CreateService(context, logger);

        Assert.Throws<AppException>(() =>
            service.DeleteTour(999));

        logger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Never());
    }
}
