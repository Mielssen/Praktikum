using System.Net;
using System.Net.Http.Json;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Tests.ToderikoTests.Helpers;
using Xunit;

namespace Praktikum542.Tests.ToderikoTests.RestApiTests;

public class PasswordResetApiTests : IClassFixture<PasswordResetWebApplicationFactory>
{
    private readonly PasswordResetWebApplicationFactory _factory;

    public PasswordResetApiTests(PasswordResetWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnOkWithGenericMessage_WhenEmailIsNotRegistered()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new ForgotPasswordDto { Email = "nobody.api@example.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Якщо email існує", content);

        Assert.Empty(_factory.Emails.SentTo("nobody.api@example.com"));
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnBadRequest_WhenEmailFormatIsInvalid()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new ForgotPasswordDto { Email = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal("INVALID_EMAIL", error.Code);
        Assert.Equal("Невірний формат пошти", error.Message);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenTokenDoesNotExist()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new ResetPasswordDto
            {
                Token = "token-that-does-not-exist",
                NewPassword = "ValidPass1"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal("INVALID_TOKEN", error.Code);
        Assert.Equal("Токен недійсний або прострочений", error.Message);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenNewPasswordIsTooShort()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new ResetPasswordDto
            {
                Token = "any-token",
                NewPassword = "123"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal("INVALID_PASSWORD", error.Code);
    }
}