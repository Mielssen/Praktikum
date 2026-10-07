using System.Linq;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.KushnirukTests.UnitTests;

public class BookingPaginationTests
{
    [Fact]
    public void ApplyPagination_ShouldReturnFirstPageWithCorrectItems_WhenPageAndPageSizeAreValid()
    {
        var data = Enumerable.Range(1, 10).ToList();

        var result = data.ApplyPagination(1, 3);

        Assert.Equal(1, result.Page);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(10, result.TotalItems);
        Assert.Equal(4, result.TotalPages);
        Assert.Equal(new[] { 1, 2, 3 }, result.Items);
    }
}