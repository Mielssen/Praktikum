using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Moq;
using Praktikum542.DTOs;
using Praktikum542.Models;
using Praktikum542.Services;
using Xunit;
using static Praktikum542.Tests.ToderikoTests.Helpers.PasswordResetTestHelpers;

namespace Praktikum542.Tests.ToderikoTests.UnitTests;

public class PasswordResetUnitTests
{
    [Fact]
    public async Task ForgotPassword_ShouldStoreHashedToken_NotRawToken()
    {
        var options = NewDbOptions();
        SeedUser(options, 9101, "unit.hash@example.com", "OldPassword1");

        string? emailBody = null;
        var emailMock = new Mock<IEmailService>();
        emailMock
            .Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((_, _, body) => emailBody = body)
            .Returns(Task.CompletedTask);

        var service = CreateService(options, emailMock.Object);

        await service.ForgotPassword(new ForgotPasswordDto { Email = "unit.hash@example.com" });

        Assert.NotNull(emailBody);
        var rawToken = ExtractToken(emailBody!);

        using var verifyContext = new PraktikumContext(options);
        var stored = Assert.Single(verifyContext.PasswordResetTokens);

        Assert.NotEqual(rawToken, stored.Token);
        Assert.Equal(HashToken(rawToken), stored.Token);
        Assert.False(stored.Used);
    }

    [Fact]
    public void ResetPassword_ShouldUpdatePasswordAndMarkTokenUsed_WhenTokenIsValid()
    {
        var options = NewDbOptions();
        SeedUser(options, 9102, "unit.reset@example.com", "OldPassword1");
        SeedToken(options, 9102, "valid-raw-token", DateTime.Now.AddMinutes(30));

        var service = CreateService(options, Mock.Of<IEmailService>());

        service.ResetPassword(new ResetPasswordDto
        {
            Token = "valid-raw-token",
            NewPassword = "BrandNewPass1"
        });

        using var verifyContext = new PraktikumContext(options);

        var user = verifyContext.Credentials.Single(c => c.CredentialId == 9102);
        Assert.True(BCrypt.Net.BCrypt.Verify("BrandNewPass1", user.PasswordHash));
        Assert.False(BCrypt.Net.BCrypt.Verify("OldPassword1", user.PasswordHash));

        var token = verifyContext.PasswordResetTokens.Single();
        Assert.True(token.Used);
    }

    [Fact]
    public async Task ForgotPassword_ShouldInvalidatePreviousTokens_WhenRequestedTwice()
    {
        var options = NewDbOptions();
        SeedUser(options, 9103, "unit.twice@example.com", "OldPassword1");

        var service = CreateService(options, Mock.Of<IEmailService>());
        var dto = new ForgotPasswordDto { Email = "unit.twice@example.com" };

        await service.ForgotPassword(dto);
        await service.ForgotPassword(dto);

        using var verifyContext = new PraktikumContext(options);
        var tokens = verifyContext.PasswordResetTokens
            .Where(t => t.CredentialId == 9103)
            .ToList();

        Assert.Equal(2, tokens.Count);
        Assert.Single(tokens, t => !t.Used);
        Assert.Single(tokens, t => t.Used);
    }

    [Fact]
    public void HashToken_ShouldBeDeterministic_AndDifferentFromInput()
    {
        var method = typeof(AuthentificationService).GetMethod(
            "HashToken",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var first = (string)method!.Invoke(null, new object[] { "some-token" })!;
        var second = (string)method.Invoke(null, new object[] { "some-token" })!;
        var other = (string)method.Invoke(null, new object[] { "another-token" })!;

        Assert.Equal(first, second);
        Assert.NotEqual(first, other);
        Assert.NotEqual("some-token", first);
        Assert.Equal(44, first.Length); // SHA-256 у Base64 = 44 символи
    }
}