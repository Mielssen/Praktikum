using Microsoft.Extensions.Logging.Abstractions;
using Praktikum542.DTOs;
using Praktikum542.Exceptions;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.KushnirukTests.ExceptionTests;

public class AuthenticationExceptionTests
{
    [Fact]
    public void ResetPassword_ShouldThrowAppException_WhenTokenIsEmpty()
    {
        var service = new AuthentificationService(
            null!,
            null!,
            null!,
            null!,
            NullLogger<AuthentificationService>.Instance);

        var dto = new ResetPasswordDto
        {
            Token = "",
            NewPassword = "Password123"
        };

        var exception = Assert.Throws<AppException>(() =>
            service.ResetPassword(dto));

        Assert.Equal("INVALID_TOKEN", exception.Code);
        Assert.Equal("Токен обов'язковий", exception.Message);
    }

    [Fact]
    public void ResetPassword_ShouldThrowAppException_WhenNewPasswordIsTooShort()
    {
        var service = new AuthentificationService(
            null!,
            null!,
            null!,
            null!,
            NullLogger<AuthentificationService>.Instance);

        var dto = new ResetPasswordDto
        {
            Token = "valid-token",
            NewPassword = "123"
        };

        var exception = Assert.Throws<AppException>(() =>
            service.ResetPassword(dto));

        Assert.Equal("INVALID_PASSWORD", exception.Code);
        Assert.Equal("Пароль має містити мінімум 6 символів", exception.Message);
    }
    [Fact]
    public void ChangePassword_ShouldThrowAppException_WhenPasswordsDoNotMatch()
    {
        var service = new AuthentificationService(
            null!,
            null!,
            null!,
            null!,
            NullLogger<AuthentificationService>.Instance);

        var dto = new ChangePasswordDto
        {
            OldPassword = "OldPassword123",
            NewPassword = "NewPassword123",
            ConfirmPassword = "AnotherPassword123"
        };

        var exception = Assert.Throws<AppException>(() =>
            service.ChangePassword(1, dto));

        Assert.Equal("PASSWORD_MISMATCH", exception.Code);
        Assert.Equal("Нові паролі не співпадають", exception.Message);
    }

}