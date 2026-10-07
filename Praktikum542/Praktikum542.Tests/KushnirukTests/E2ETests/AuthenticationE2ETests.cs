using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Tests.KushnirukTests.IntegrationTests;
using Xunit;

namespace Praktikum542.Tests.KushnirukTests.E2ETests;

public class AuthenticationE2ETests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthenticationE2ETests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task User_ShouldLoginAndAccessProfile_WhenCredentialsAreValid()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<PraktikumContext>();

            var user = new Credential
            {
                CredentialId = 600,
                Email = "e2euser@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                Role = "client",
                Status = "Active",
                CreatedAt = DateTime.Now,
                MustChangePassword = false
            };

            var detail = new UserDetail
            {
                UserId = 600,
                CredentialId = 600,
                Name = "E2E Test User",
                Phone = "+380501112233",
                PassportData = "987654321",
                DateOfBirth = new DateOnly(2000, 5, 15),
                AvatarUrl = null
            };

            context.Credentials.Add(user);
            context.UserDetails.Add(detail);
            context.SaveChanges();
        }

        var client = _factory.CreateClient();

        var loginDto = new LoginDto
        {
            Email = "e2euser@example.com",
            Password = "Password123"
        };

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            loginDto);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var loginResult = await loginResponse.Content
            .ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(loginResult);
        Assert.False(
            string.IsNullOrWhiteSpace(loginResult.Token));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.Token);

        var profileResponse = await client.GetAsync(
            "/api/auth/profile");

        Assert.Equal(
            HttpStatusCode.OK,
            profileResponse.StatusCode);

        var profile = await profileResponse.Content
            .ReadFromJsonAsync<ProfileResponse>();

        Assert.NotNull(profile);

        Assert.Equal(
            "e2euser@example.com",
            profile.Email);

        Assert.Equal(
            "E2E Test User",
            profile.Name);

        Assert.Equal(
            "+380501112233",
            profile.Phone);

        Assert.Equal(
            "987654321",
            profile.PassportData);
    }

    private class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
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