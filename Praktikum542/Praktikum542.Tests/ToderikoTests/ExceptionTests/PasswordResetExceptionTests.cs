using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Praktikum542.DTOs;
using Praktikum542.Exceptions;
using Praktikum542.Services;
using Xunit;
using static Praktikum542.Tests.ToderikoTests.Helpers.PasswordResetTestHelpers;

namespace Praktikum542.Tests.ToderikoTests.ExceptionTests;

public class PasswordResetExceptionTests
{
    [Fact]
    public async Task ForgotPassword_ShouldThrowAppException_WhenEmailFormatIsInvalid()
    {
        var service = new AuthentificationService(
            null!, null!, null!, null!,
            NullLogger<AuthentificationService>.Instance);

        var exception = await Assert.ThrowsAsync<AppException>(() =>
            service.ForgotPassword(new ForgotPasswordDto { Email = "not-an-email" }));

        Assert.Equal("INVALID_EMAIL", exception.Code);
        Assert.Equal("Невірний формат пошти", exception.Message);
    }

    [Fact]
    public async Task ForgotPassword_ShouldThrowAppException_WhenEmailIsBlank()
    {
        var service = new AuthentificationService(
            null!, null!, null!, null!,
            NullLogger<AuthentificationService>.Instance);

        var exception = await Assert.ThrowsAsync<AppException>(() =>
            service.ForgotPassword(new ForgotPasswordDto { Email = "   " }));

        Assert.Equal("INVALID_EMAIL", exception.Code);
        Assert.Equal("Email обов'язковий", exception.Message);
    }

    [Fact]
    public void ResetPassword_ShouldThrowAppException_WhenTokenDoesNotExist()
    {
        var options = NewDbOptions();
        var service = CreateService(options, Mock.Of<IEmailService>());

        var exception = Assert.Throws<AppException>(() =>
            service.ResetPassword(new ResetPasswordDto
            {
                Token = "token-that-was-never-issued",
                NewPassword = "ValidPass1"
            }));

        Assert.Equal("INVALID_TOKEN", exception.Code);
        Assert.Equal("Токен недійсний або прострочений", exception.Message);
    }

    [Fact]
    public void ResetPassword_ShouldThrowAppException_WhenTokenIsExpired()
    {
        var options = NewDbOptions();
        SeedUser(options, 9201, "exception.expired@example.com", "OldPassword1");
        SeedToken(options, 9201, "expired-token", DateTime.Now.AddMinutes(-1));

        var service = CreateService(options, Mock.Of<IEmailService>());

        var exception = Assert.Throws<AppException>(() =>
            service.ResetPassword(new ResetPasswordDto
            {
                Token = "expired-token",
                NewPassword = "ValidPass1"
            }));

        Assert.Equal("INVALID_TOKEN", exception.Code);
    }

    [Fact]
    public void ResetPassword_ShouldThrowAppException_WhenTokenWasAlreadyUsed()
    {
        var options = NewDbOptions();
        SeedUser(options, 9202, "exception.used@example.com", "OldPassword1");
        SeedToken(options, 9202, "used-token", DateTime.Now.AddMinutes(30), used: true);

        var service = CreateService(options, Mock.Of<IEmailService>());

        var exception = Assert.Throws<AppException>(() =>
            service.ResetPassword(new ResetPasswordDto
            {
                Token = "used-token",
                NewPassword = "ValidPass1"
            }));

        Assert.Equal("INVALID_TOKEN", exception.Code);
    }
}