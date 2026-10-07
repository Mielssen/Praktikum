using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Praktikum542.Models;
using Praktikum542.Services;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace Praktikum542.Tests.KushnirukTests.MockingTests;

public class AuthenticationMockingTests
{
    [Fact]
    public void GenerateJwtToken_ShouldReadJwtSettings_FromConfiguration()
    {
        var configurationMock = new Mock<IConfiguration>();

        configurationMock
            .Setup(x => x["Jwt:Key"])
            .Returns("TRAVELMANAGER_SECRET_KEY_123456789123456789");

        configurationMock
            .Setup(x => x["Jwt:Issuer"])
            .Returns("praktikum");

        configurationMock
            .Setup(x => x["Jwt:Audience"])
            .Returns("praktikumUsers");

        configurationMock
            .Setup(x => x["Jwt:ExpiresInMinutes"])
            .Returns("60");

        var service = new AuthentificationService(
            null!,
            null!,
            null!,
            configurationMock.Object,
            NullLogger<AuthentificationService>.Instance);

        var user = new Credential
        {
            CredentialId = 1,
            Email = "user@example.com",
            Role = "client"
        };

        var token = service.GenerateJwtToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));

        configurationMock.VerifyGet(
            x => x["Jwt:Key"],
            Times.Once);

        configurationMock.VerifyGet(
            x => x["Jwt:Issuer"],
            Times.Once);

        configurationMock.VerifyGet(
            x => x["Jwt:Audience"],
            Times.Once);

        configurationMock.VerifyGet(
            x => x["Jwt:ExpiresInMinutes"],
            Times.Once);
    }

    [Fact]
    public void GenerateJwtToken_ShouldUseIssuerAndAudience_FromMockedConfiguration()
    {
        var configurationMock = new Mock<IConfiguration>();

        configurationMock
            .Setup(x => x["Jwt:Key"])
            .Returns("TRAVELMANAGER_SECRET_KEY_123456789123456789");

        configurationMock
            .Setup(x => x["Jwt:Issuer"])
            .Returns("test-issuer");

        configurationMock
            .Setup(x => x["Jwt:Audience"])
            .Returns("test-audience");

        configurationMock
            .Setup(x => x["Jwt:ExpiresInMinutes"])
            .Returns("60");

        var service = new AuthentificationService(
            null!,
            null!,
            null!,
            configurationMock.Object,
            NullLogger<AuthentificationService>.Instance);

        var user = new Credential
        {
            CredentialId = 10,
            Email = "mock@example.com",
            Role = "client"
        };

        var token = service.GenerateJwtToken(user);

        var jwtToken = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        Assert.Equal("test-issuer", jwtToken.Issuer);
        Assert.Contains("test-audience", jwtToken.Audiences);

        configurationMock.VerifyGet(
            x => x["Jwt:Issuer"],
            Times.Once);

        configurationMock.VerifyGet(
            x => x["Jwt:Audience"],
            Times.Once);
    }
    [Fact]
    public void GenerateJwtToken_ShouldUseExpirationTime_FromMockedConfiguration()
    {
        var configurationMock = new Mock<IConfiguration>();

        configurationMock
            .Setup(x => x["Jwt:Key"])
            .Returns("TRAVELMANAGER_SECRET_KEY_123456789123456789");

        configurationMock
            .Setup(x => x["Jwt:Issuer"])
            .Returns("praktikum");

        configurationMock
            .Setup(x => x["Jwt:Audience"])
            .Returns("praktikumUsers");

        configurationMock
            .Setup(x => x["Jwt:ExpiresInMinutes"])
            .Returns("120");

        var service = new AuthentificationService(
            null!,
            null!,
            null!,
            configurationMock.Object,
            NullLogger<AuthentificationService>.Instance);

        var user = new Credential
        {
            CredentialId = 15,
            Email = "expiration@example.com",
            Role = "client"
        };

        var beforeGeneration = DateTime.UtcNow;

        var token = service.GenerateJwtToken(user);

        var afterGeneration = DateTime.UtcNow;

        var jwtToken = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        var expectedMinimumExpiration = beforeGeneration.AddMinutes(119);
        var expectedMaximumExpiration = afterGeneration.AddMinutes(121);

        Assert.InRange(
            jwtToken.ValidTo,
            expectedMinimumExpiration,
            expectedMaximumExpiration);

        configurationMock.VerifyGet(
            x => x["Jwt:ExpiresInMinutes"],
            Times.Once);
    }
    [Fact]
    public void GenerateJwtToken_ShouldBeSignedWithKey_FromMockedConfiguration()
    {
        var signingKey = "TRAVELMANAGER_MOCK_SECRET_KEY_123456789123456789";

        var configurationMock = new Mock<IConfiguration>();

        configurationMock
            .Setup(x => x["Jwt:Key"])
            .Returns(signingKey);

        configurationMock
            .Setup(x => x["Jwt:Issuer"])
            .Returns("praktikum");

        configurationMock
            .Setup(x => x["Jwt:Audience"])
            .Returns("praktikumUsers");

        configurationMock
            .Setup(x => x["Jwt:ExpiresInMinutes"])
            .Returns("60");

        var service = new AuthentificationService(
            null!,
            null!,
            null!,
            configurationMock.Object,
            NullLogger<AuthentificationService>.Instance);

        var user = new Credential
        {
            CredentialId = 20,
            Email = "signature@example.com",
            Role = "client"
        };

        var token = service.GenerateJwtToken(user);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(signingKey)),

            ValidateIssuer = true,
            ValidIssuer = "praktikum",

            ValidateAudience = true,
            ValidAudience = "praktikumUsers",

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();

        var principal = handler.ValidateToken(
            token,
            validationParameters,
            out var validatedToken);

        Assert.NotNull(principal);
        Assert.NotNull(validatedToken);

        configurationMock.VerifyGet(
            x => x["Jwt:Key"],
            Times.Once);
    }
    [Fact]
    public void GenerateJwtToken_ShouldThrowFormatException_WhenExpirationFromMockIsInvalid()
    {
        var configurationMock = new Mock<IConfiguration>();

        configurationMock
            .Setup(x => x["Jwt:Key"])
            .Returns("TRAVELMANAGER_SECRET_KEY_123456789123456789");

        configurationMock
            .Setup(x => x["Jwt:Issuer"])
            .Returns("praktikum");

        configurationMock
            .Setup(x => x["Jwt:Audience"])
            .Returns("praktikumUsers");

        configurationMock
            .Setup(x => x["Jwt:ExpiresInMinutes"])
            .Returns("abc");

        var service = new AuthentificationService(
            null!,
            null!,
            null!,
            configurationMock.Object,
            NullLogger<AuthentificationService>.Instance);

        var user = new Credential
        {
            CredentialId = 30,
            Email = "invalid-config@example.com",
            Role = "client"
        };

        Assert.Throws<FormatException>(() =>
            service.GenerateJwtToken(user));

        configurationMock.VerifyGet(
            x => x["Jwt:ExpiresInMinutes"],
            Times.Once);
    }
}