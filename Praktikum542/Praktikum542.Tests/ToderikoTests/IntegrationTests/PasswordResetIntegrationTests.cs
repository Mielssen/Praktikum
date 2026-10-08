using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Tests.ToderikoTests.Helpers;
using Xunit;
using static Praktikum542.Tests.ToderikoTests.Helpers.PasswordResetTestHelpers;

namespace Praktikum542.Tests.ToderikoTests.IntegrationTests;

public class PasswordResetIntegrationTests : IClassFixture<PasswordResetWebApplicationFactory>
{
    private readonly PasswordResetWebApplicationFactory _factory;

    public PasswordResetIntegrationTests(PasswordResetWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ForgotPassword_ShouldPersistOnlyTokenHash_InDatabase()
    {
        const string email = "integration.hash@example.com";

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PraktikumContext>();
            SeedUser(context, 9401, email, "OldPassword1");
        }

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new ForgotPasswordDto { Email = email });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var sentEmail = Assert.Single(_factory.Emails.SentTo(email));
        var rawToken = ExtractToken(sentEmail.Body);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PraktikumContext>();
            var stored = context.PasswordResetTokens.Single(t => t.CredentialId == 9401);

            Assert.NotEqual(rawToken, stored.Token);
            Assert.Equal(HashToken(rawToken), stored.Token);
            Assert.False(stored.Used);
            Assert.True(stored.ExpiresAt > DateTime.Now);
        }
    }

    [Fact]
    public async Task ResetPassword_ShouldUpdatePasswordHash_AndMarkTokenAsUsed()
    {
        const string email = "integration.reset@example.com";

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PraktikumContext>();
            SeedUser(context, 9402, email, "OldPassword1");
        }

        var client = _factory.CreateClient();

        await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new ForgotPasswordDto { Email = email });

        var rawToken = ExtractToken(
            Assert.Single(_factory.Emails.SentTo(email)).Body);

        var resetResponse = await client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new ResetPasswordDto { Token = rawToken, NewPassword = "BrandNewPass1" });

        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PraktikumContext>();

            var user = context.Credentials.Single(c => c.CredentialId == 9402);
            Assert.True(BCrypt.Net.BCrypt.Verify("BrandNewPass1", user.PasswordHash));
            Assert.False(BCrypt.Net.BCrypt.Verify("OldPassword1", user.PasswordHash));

            var token = context.PasswordResetTokens.Single(t => t.CredentialId == 9402);
            Assert.True(token.Used);
        }
    }
}