
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Praktikum542.Models;
using Xunit;

namespace Praktikum542.Tests.KasapchukTests.RestApiTests;

public class ProtectedResourcesApiTests
{
    private sealed class RestApiTestFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName =
            "RestApiTests_" + Guid.NewGuid().ToString("N");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<PraktikumContext>>();

                services.AddDbContext<PraktikumContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }

    [Fact]
    public async Task GetFavorites_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        using var factory = new RestApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/favorites");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddFavorite_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        using var factory = new RestApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/favorites",
            new { tourId = 1 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFavorite_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        using var factory = new RestApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/favorites/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSavedPersons_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        using var factory = new RestApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/savedpersons");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddSavedPerson_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        using var factory = new RestApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/savedpersons",
            new
            {
                name = "Ivan Petrenko",
                passportData = "123456789",
                dateOfBirth = "2000-05-10",
                isChild = false
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
