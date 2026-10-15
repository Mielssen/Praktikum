
using System.Collections.Generic;
using System.Linq;
using Praktikum542.Models;
using Praktikum542.Services;
using Xunit;

namespace Praktikum542.Tests.KasapchukTests.UnitTests;

public class FilteringAndPaginationTests
{
    [Fact]
    public void ApplySearch_ShouldFindUserByName_WhenNameMatches()
    {
        var users = new List<Credential>
        {
            new Credential
            {
                CredentialId = 1,
                Email = "first@example.com",
                UserDetail = new UserDetail
                {
                    Name = "Oleksandr Kovalenko"
                }
            },
            new Credential
            {
                CredentialId = 2,
                Email = "second@example.com",
                UserDetail = new UserDetail
                {
                    Name = "Maria Shevchenko"
                }
            }
        }.AsQueryable();

        var result = users.ApplySearch("maria").ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].CredentialId);
    }

    [Fact]
    public void ApplySearch_ShouldIgnoreSpaces_WhenSearchHasExtraSpaces()
    {
        var users = new List<Credential>
        {
            new Credential
            {
                CredentialId = 1,
                Email = "travel@example.com"
            },
            new Credential
            {
                CredentialId = 2,
                Email = "hotel@example.com"
            }
        }.AsQueryable();

        var result = users.ApplySearch("  TRAVEL  ").ToList();

        Assert.Single(result);
        Assert.Equal("travel@example.com", result[0].Email);
    }

    [Fact]
    public void ApplySearch_ShouldReturnAllUsers_WhenSearchIsEmpty()
    {
        var users = new List<Credential>
        {
            new Credential { CredentialId = 1, Email = "a@test.com" },
            new Credential { CredentialId = 2, Email = "b@test.com" },
            new Credential { CredentialId = 3, Email = "c@test.com" }
        }.AsQueryable();

        var result = users.ApplySearch("").ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplyPagination_ShouldUseFirstPage_WhenPageIsNegative()
    {
        var numbers = Enumerable.Range(1, 12);

        var result = numbers.ApplyPagination(-3, 5);

        Assert.Equal(1, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.Items);
    }

    [Fact]
    public void ApplyPagination_ShouldReturnEmptyItems_WhenPageExceedsTotalPages()
    {
        var numbers = Enumerable.Range(1, 7);

        var result = numbers.ApplyPagination(10, 3);

        Assert.Empty(result.Items);
        Assert.Equal(7, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(10, result.Page);
    }
}
