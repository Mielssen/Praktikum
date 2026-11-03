
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.SmetanyukTests;

public class ReliabilityTests
{
    [Fact]
    public void FavoriteAdd_DatabaseFails_ThrowsException()
    {
        using var context = TestHelper.CreateContext();

        var repository = new Mock<FavoriteRepository>(context);

        repository.Setup(r => r.TourExists(1))
            .Returns(true);

        repository.Setup(r => r.GetByCredentialAndTour(1, 1))
            .Returns((Favorite?)null);

        repository.Setup(r => r.Add(It.IsAny<Favorite>()))
            .Throws(new DbUpdateException("Database is unavailable"));

        var service = new FavoriteService(repository.Object);

        Assert.Throws<DbUpdateException>(() => service.Add(1, 1));
    }

    [Fact]
    public async Task ForgotPassword_SmtpFails_ThrowsException()
    {
        using var context = TestHelper.CreateContext();

        context.Credentials.Add(new Credential
        {
            CredentialId = 1,
            Email = "test@example.com",
            PasswordHash = "test-hash",
            Role = "client",
            Status = "Active",
            CreatedAt = DateTime.Now
        });

        context.SaveChanges();

        var emailService = new Mock<IEmailService>();

        emailService
            .Setup(e => e.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new SmtpException("SMTP server unavailable"));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Frontend:ResetPasswordUrl"] = "https://example.com/reset"
            })
            .Build();

        var service = new AuthentificationService(
            new CredentialsRepository(context),
            new PasswordResetRepository(context),
            emailService.Object,
            configuration,
            NullLogger<AuthentificationService>.Instance);

        await Assert.ThrowsAsync<SmtpException>(() =>
            service.ForgotPassword(new Praktikum542.DTOs.ForgotPasswordDto
            {
                Email = "test@example.com"
            }));

        emailService.Verify(e => e.SendAsync(
            "test@example.com",
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }
}
