using Microsoft.Extensions.Logging.Abstractions;
using Praktikum542.DTOs;
using Praktikum542.Exceptions;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.KushnirukTests.ExceptionTests;

public class ReviewExceptionTests
{
    [Fact]
    public void Create_ShouldThrowAppException_WhenRatingIsInvalid()
    {
        var service = new ReviewService(
            null!,
            NullLogger<ReviewService>.Instance);

        var dto = new CreateReviewDto
        {
            BookingId = 1,
            Rating = 6,
            Comment = "Test review"
        };

        var exception = Assert.Throws<AppException>(() =>
            service.Create(1, dto));

        Assert.Equal("INVALID_RATING", exception.Code);
        Assert.Equal("Оцінка повинна бути від 1 до 5", exception.Message);
    }
}