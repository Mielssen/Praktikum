using Microsoft.Extensions.Logging.Abstractions;
using Praktikum542.Exceptions;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.KushnirukTests.ExceptionTests;

public class TourExceptionTests
{
    [Fact]
    public void CreateTour_ShouldThrowAppException_WhenDtoIsNull()
    {
        var service = new TourService(
            null!,
            null!,
            NullLogger<TourService>.Instance);

        var exception = Assert.Throws<AppException>(() =>
            service.CreateTour(null!));

        Assert.Equal("NULL", exception.Code);
        Assert.Equal("Дані не передані", exception.Message);
    }
}