using Praktikum542.Tests.KushnirukTests.IntegrationTests;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Praktikum542.DTOs;
namespace Praktikum542.Tests.KushnirukTests.RestApiTests;

public class TourApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public TourApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTours_ShouldReturnOk_WhenRequestIsValid()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tours");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(
            "application/json",
            response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content.ReadAsStringAsync();

        Assert.False(string.IsNullOrWhiteSpace(content));
    }
    [Fact]
    public async Task CreateTour_ShouldReturnUnauthorized_WhenJwtIsMissing()
    {
        var client = _factory.CreateClient();

        var dto = new CreateTourDto
        {
            Name = "Unauthorized Tour",
            Description = "REST API test",
            Price = 2500,
            DurationDays = 4,
            AvailableFrom = new DateOnly(2026, 11, 1),
            AvailableTo = new DateOnly(2026, 12, 1),
            TypeId = 1
        };

        var response = await client.PostAsJsonAsync(
            "/api/tours",
            dto);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}