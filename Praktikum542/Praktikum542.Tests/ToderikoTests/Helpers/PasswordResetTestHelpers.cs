using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Praktikum542.Models;
using Praktikum542.Repositories;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.ToderikoTests.Helpers;

public static class PasswordResetTestHelpers
{
    public const string ResetUrl = "http://test-frontend/reset-password.html";

    /// <summary>Нова ізольована in-memory БД (унікальна назва на кожен тест).</summary>
    public static DbContextOptions<PraktikumContext> NewDbOptions() =>
        new DbContextOptionsBuilder<PraktikumContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    public static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Frontend:ResetPasswordUrl"] = ResetUrl,
                ["Jwt:Key"] = "TRAVELMANAGER_SECRET_KEY_123456789123456789",
                ["Jwt:Issuer"] = "praktikum",
                ["Jwt:Audience"] = "praktikumUsers",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

    /// <summary>
    /// Справжній сервіс зі справжніми репозиторіями поверх in-memory БД.
    /// Мокається тільки email (IEmailService) та, за потреби, IConfiguration.
    /// </summary>
    public static AuthentificationService CreateService(
        DbContextOptions<PraktikumContext> options,
        IEmailService emailService,
        IConfiguration? configuration = null)
    {
        var context = new PraktikumContext(options);

        return new AuthentificationService(
            new CredentialsRepository(context),
            new PasswordResetRepository(context),
            emailService,
            configuration ?? BuildConfig(),
            NullLogger<AuthentificationService>.Instance);
    }

    public static void SeedUser(
        DbContextOptions<PraktikumContext> options,
        int id, string email, string password)
    {
        using var context = new PraktikumContext(options);
        SeedUser(context, id, email, password);
    }

    public static void SeedUser(
        PraktikumContext context,
        int id, string email, string password)
    {
        context.Credentials.Add(new Credential
        {
            CredentialId = id,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "client",
            Status = "Active",
            CreatedAt = DateTime.Now,
            MustChangePassword = false
        });

        context.SaveChanges();
    }

    /// <summary>Кладе в БД токен скидання (у вигляді хешу, як це робить сервіс).</summary>
    public static void SeedToken(
        DbContextOptions<PraktikumContext> options,
        int credentialId, string rawToken,
        DateTime expiresAt, bool used = false)
    {
        using var context = new PraktikumContext(options);

        context.PasswordResetTokens.Add(new PasswordResetToken
        {
            CredentialId = credentialId,
            Token = HashToken(rawToken),
            ExpiresAt = expiresAt,
            Used = used,
            CreatedAt = DateTime.Now
        });

        context.SaveChanges();
    }

    /// <summary>Той самий алгоритм, що і в сервісі: SHA-256 → Base64.</summary>
    public static string HashToken(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    /// <summary>Витягує "сирий" токен з посилання в тілі листа.</summary>
    public static string ExtractToken(string emailBody)
    {
        var match = Regex.Match(emailBody, @"token=([A-Za-z0-9_\-]+)");
        Assert.True(match.Success, "У тілі листа не знайдено токен у посиланні");
        return match.Groups[1].Value;
    }
}