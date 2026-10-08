using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Tests.ToderikoTests.Helpers;
using Xunit;
using static Praktikum542.Tests.ToderikoTests.Helpers.PasswordResetTestHelpers;

namespace Praktikum542.Tests.ToderikoTests.E2ETests;

public class PasswordResetE2ETests : IClassFixture<PasswordResetWebApplicationFactory>
{
    private readonly PasswordResetWebApplicationFactory _factory;

    public PasswordResetE2ETests(PasswordResetWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task User_ShouldRegister_ResetForgottenPassword_AndLoginWithNewPassword()
    {
        const string email = "e2e.reset@example.com";
        const string oldPassword = "OldPassword1";
        const string newPassword = "BrandNewPass1";

        var client = _factory.CreateClient();

        var formData = new MultipartFormDataContent
        {
            { new StringContent(email), "Email" },
            { new StringContent(oldPassword), "Password" },
            { new StringContent("Dmytro"), "Name" },
            { new StringContent("0501234567"), "Phone" },
            { new StringContent("123456789"), "PassportData" },
            { new StringContent("2000-03-15"), "DateOfBirth" }
        };

        var registerResponse = await client.PostAsync("/api/auth/register", formData);

        await AssertStatusAsync(HttpStatusCode.OK, registerResponse, "1. register");

        var forgotResponse = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new ForgotPasswordDto { Email = email });

        await AssertStatusAsync(HttpStatusCode.OK, forgotResponse, "2. forgot-password");

        var sentEmail = Assert.Single(_factory.Emails.SentTo(email));

        Assert.Contains("reset-password", sentEmail.Body, StringComparison.OrdinalIgnoreCase);
        var token = ExtractToken(sentEmail.Body);
        Assert.False(string.IsNullOrWhiteSpace(token), "Токен скидання пароля не знайдено в тілі листа.");

        var resetResponse = await client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new ResetPasswordDto { Token = token, NewPassword = newPassword });

        await AssertStatusAsync(HttpStatusCode.OK, resetResponse, "3. reset-password");

        var oldLogin = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginDto { Email = email, Password = oldPassword });

        await AssertStatusAsync(HttpStatusCode.BadRequest, oldLogin, "4. login зі старим паролем");
        var oldLoginError = await oldLogin.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("WRONG_PASSWORD", oldLoginError!.Code);

        var newLogin = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginDto { Email = email, Password = newPassword });

        await AssertStatusAsync(HttpStatusCode.OK, newLogin, "5. login з новим паролем");

        var loginResult = await newLogin.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.False(string.IsNullOrWhiteSpace(loginResult!.Token));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

        var profileResponse = await client.GetAsync("/api/auth/profile");
        await AssertStatusAsync(HttpStatusCode.OK, profileResponse, "6. profile");

        var profile = await profileResponse.Content.ReadFromJsonAsync<ProfileResponse>();
        Assert.Equal(email, profile!.Email);

        client.DefaultRequestHeaders.Authorization = null;

        var replay = await client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new ResetPasswordDto { Token = token, NewPassword = "AnotherPass123" });

        await AssertStatusAsync(HttpStatusCode.BadRequest, replay, "7. повторний reset-password");
        var replayError = await replay.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("INVALID_TOKEN", replayError!.Code);
    }

    private static async Task AssertStatusAsync(
        HttpStatusCode expected, HttpResponseMessage response, string step)
    {
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == expected,
            $"[{step}] очікувався {expected}, а отримано {response.StatusCode}. Тіло відповіді: {body}");
    }

    private class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    private class ProfileResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}