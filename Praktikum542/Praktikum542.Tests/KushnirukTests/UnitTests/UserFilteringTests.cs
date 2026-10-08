using System.Collections.Generic;
using System.Linq;
using Praktikum542.Models;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.KushnirukTests.UnitTests;

public class UserFilteringTests
{
    [Fact]
    public void ApplySearch_ShouldReturnUser_WhenEmailContainsSearchTerm()
    {
        var users = new List<Credential>
        {
            new Credential
            {
                CredentialId = 1,
                Email = "anna@example.com"
            },
            new Credential
            {
                CredentialId = 2,
                Email = "ivan@example.com"
            },
            new Credential
            {
                CredentialId = 3,
                Email = "petro@example.com"
            }
        }.AsQueryable();

        var result = users.ApplySearch("ivan").ToList();

        Assert.Single(result);
        Assert.Equal("ivan@example.com", result[0].Email);
    }
}