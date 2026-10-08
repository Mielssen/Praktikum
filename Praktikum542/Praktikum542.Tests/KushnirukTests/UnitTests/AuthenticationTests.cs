using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Praktikum542.Models;
using Praktikum542.Services;
using Xunit;
using System.Reflection;
namespace Praktikum542.Tests.KushnirukTests.UnitTests;

public class AuthenticationTests
{
    [Fact]
    public void GenerateJwtToken_ShouldNotContainPasswordHash_WhenTokenIsCreated()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "TRAVELMANAGER_SECRET_KEY_123456789123456789",
            ["Jwt:Issuer"] = "praktikum",
            ["Jwt:Audience"] = "praktikumUsers",
            ["Jwt:ExpiresInMinutes"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var service = new AuthentificationService(
            null!,
            null!,
            null!,
            configuration,
            NullLogger<AuthentificationService>.Instance);

        var user = new Credential
        {
            CredentialId = 1,
            Email = "user@example.com",
            Role = "client",
            PasswordHash = "secret_password_hash"
        };

        var token = service.GenerateJwtToken(user);

        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.DoesNotContain(
            jwtToken.Claims,
            claim => claim.Value == user.PasswordHash);
    }


    [Fact]
    public void GenerateJwtToken_ShouldContainCorrectUserData_WhenTokenIsCreated()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "TRAVELMANAGER_SECRET_KEY_123456789123456789",
            ["Jwt:Issuer"] = "praktikum",
            ["Jwt:Audience"] = "praktikumUsers",
            ["Jwt:ExpiresInMinutes"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var service = new AuthentificationService(
            null!,
            null!,
            null!,
            configuration,
            NullLogger<AuthentificationService>.Instance);

        var user = new Credential
        {
            CredentialId = 25,
            Email = "client@example.com",
            Role = "client",
            PasswordHash = "secret_password_hash"
        };

        var token = service.GenerateJwtToken(user);

        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("praktikum", jwtToken.Issuer);
        Assert.Contains("praktikumUsers", jwtToken.Audiences);

        Assert.Contains(
            jwtToken.Claims,
            claim => claim.Value == user.CredentialId.ToString());

        Assert.Contains(
            jwtToken.Claims,
            claim => claim.Value == user.Email);

        Assert.Contains(
            jwtToken.Claims,
            claim => claim.Value == user.Role);
    }
    [Fact]
    public void NormalizePhone_ShouldAddCountryCode_WhenPhoneContainsTenDigits()
    {
        var service = new AuthentificationService(
            null!,
            null!,
            null!,
            null!,
            NullLogger<AuthentificationService>.Instance);

        var method = typeof(AuthentificationService)
            .GetMethod(
                "NormalizePhone",
                BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);

        var result = method.Invoke(
            service,
            new object[] { "5012345678" });

        Assert.Equal("+385012345678", result);
    }
}

