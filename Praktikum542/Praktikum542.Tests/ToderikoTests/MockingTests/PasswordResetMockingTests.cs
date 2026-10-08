using Microsoft.Extensions.Configuration;
using Moq;
using Praktikum542.DTOs;
using Praktikum542.Services;
using Xunit;
using static Praktikum542.Tests.ToderikoTests.Helpers.PasswordResetTestHelpers;

namespace Praktikum542.Tests.ToderikoTests.MockingTests;

public class PasswordResetMockingTests
{
    [Fact]
    public async Task ForgotPassword_ShouldSendEmailExactlyOnce_ToRegisteredUser()
    {
        var options = NewDbOptions();
        SeedUser(options, 9301, "mock.send@example.com", "OldPassword1");

        var emailMock = new Mock<IEmailService>();
        var service = CreateService(options, emailMock.Object);

        await service.ForgotPassword(new ForgotPasswordDto { Email = "Mock.Send@Example.com" });

        emailMock.Verify(
            x => x.SendAsync(
                "mock.send@example.com",
                "Скидання паролю",
                It.Is<string>(body => body.Contains(ResetUrl + "?token="))),
            Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_ShouldNotSendEmail_WhenUserDoesNotExist()
    {
        var options = NewDbOptions();
        var emailMock = new Mock<IEmailService>();
        var service = CreateService(options, emailMock.Object);

        await service.ForgotPassword(new ForgotPasswordDto { Email = "ghost@example.com" });

        emailMock.Verify(
            x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_ShouldBuildLink_FromMockedConfiguration()
    {
        var options = NewDbOptions();
        SeedUser(options, 9302, "mock.config@example.com", "OldPassword1");

        var configMock = new Mock<IConfiguration>();
        configMock
            .Setup(x => x["Frontend:ResetPasswordUrl"])
            .Returns("https://mocked-frontend/reset.html");

        var emailMock = new Mock<IEmailService>();
        var service = CreateService(options, emailMock.Object, configMock.Object);

        await service.ForgotPassword(new ForgotPasswordDto { Email = "mock.config@example.com" });

        emailMock.Verify(
            x => x.SendAsync(
                "mock.config@example.com",
                It.IsAny<string>(),
                It.Is<string>(body => body.Contains("https://mocked-frontend/reset.html?token="))),
            Times.Once);

        configMock.VerifyGet(x => x["Frontend:ResetPasswordUrl"], Times.Once);
    }
}