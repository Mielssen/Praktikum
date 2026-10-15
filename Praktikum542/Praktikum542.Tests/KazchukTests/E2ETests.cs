using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Praktikum542.DTOs;

namespace Praktikum542.Tests.KazchukTests
{
    public class E2ETests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public E2ETests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task E2E_ClientSecurityFlow_UnauthorizedUserCannotAccessPrivateResources()
        {
            // Сценарій 1: Неавторизований клієнт намагається отримати доступ до особистих і адміністративних ресурсів
            var client = _factory.CreateClient();

            var bookingsResponse = await client.GetAsync("/api/bookings/my");
            bookingsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var adminResponse = await client.GetAsync("/api/admin/users");
            adminResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task E2E_InvalidAuthFlow_FailsValidationBeforeDatabaseCall()
        {
            // Сценарій 2: Користувач вводить порожні облікові дані при вході
            var client = _factory.CreateClient();
            var emptyLogin = new LoginDto { Email = "", Password = "" };

            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", emptyLogin);

            loginResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}