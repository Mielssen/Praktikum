using Microsoft.Extensions.DependencyInjection;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Tests.KushnirukTests.IntegrationTests;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Praktikum542.Tests.KushnirukTests.RestApiTests;

public class AuthenticationApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthenticationApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProfile_ShouldReturnUnauthorized_WhenJwtIsMissing()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/profile");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsWrong()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<PraktikumContext>();

            var user = new Credential
            {
                CredentialId = 500,
                Email = "wrongpassword@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123"),
                Role = "client",
                Status = "Active",
                CreatedAt = DateTime.Now,
                MustChangePassword = false
            };

            context.Credentials.Add(user);
            context.SaveChanges();
        }

        var client = _factory.CreateClient();

        var dto = new LoginDto
        {
            Email = "wrongpassword@example.com",
            Password = "WrongPassword123"
        };

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            dto);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal("WRONG_PASSWORD", error.Code);
        Assert.Equal("Неправильний пароль", error.Message);
    }
    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenTokenIsEmpty()
    {
        var client = _factory.CreateClient();

        var dto = new ResetPasswordDto
        {
            Token = "",
            NewPassword = "NewPassword123"
        };

        var response = await client.PostAsJsonAsync(
            "/api/auth/reset-password",
            dto);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal("INVALID_TOKEN", error.Code);
        Assert.Equal("Токен обов'язковий", error.Message);
    }
}