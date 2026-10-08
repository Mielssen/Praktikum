using Microsoft.Extensions.DependencyInjection;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Services;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
namespace Praktikum542.Tests.KushnirukTests.IntegrationTests;

public class AuthenticationIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthenticationIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ShouldReturnJwt_WhenCredentialsExistInDatabase()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<PraktikumContext>();

            var user = new Credential
            {
                CredentialId = 200,
                Email = "integration@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                Role = "client",
                Status = "Active",
                CreatedAt = DateTime.Now,
                MustChangePassword = false
            };

            context.Credentials.Add(user);
            context.SaveChanges();
        }

        var client = _factory.CreateClient();

        var loginDto = new LoginDto
        {
            Email = "integration@example.com",
            Password = "Password123"
        };

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            loginDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    private class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }
    [Fact]
    public async Task GetProfile_ShouldReturnCurrentUserProfile_WhenJwtIsValid()
    {
        string token;

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<PraktikumContext>();

            var user = new Credential
            {
                CredentialId = 300,
                Email = "profile@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                Role = "client",
                Status = "Active",
                CreatedAt = DateTime.Now,
                MustChangePassword = false
            };

            var detail = new UserDetail
            {
                UserId = 300,
                CredentialId = 300,
                Name = "Test User",
                Phone = "+380501234567",
                PassportData = "123456789",
                DateOfBirth = new DateOnly(2000, 1, 1),
                AvatarUrl = "/avatars/test.png"
            };

            context.Credentials.Add(user);
            context.UserDetails.Add(detail);
            context.SaveChanges();

            var authService = scope.ServiceProvider
                .GetRequiredService<AuthentificationService>();

            token = authService.GenerateJwtToken(user);
        }

        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/auth/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ProfileResponse>();

        Assert.NotNull(result);
        Assert.Equal("profile@example.com", result.Email);
        Assert.Equal("Test User", result.Name);
        Assert.Equal("+380501234567", result.Phone);
        Assert.Equal("123456789", result.PassportData);
        Assert.Equal(new DateOnly(2000, 1, 1), result.DateOfBirth);
        Assert.Equal("/avatars/test.png", result.AvatarUrl);
    }
    private class ProfileResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? PassportData { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
    }
}