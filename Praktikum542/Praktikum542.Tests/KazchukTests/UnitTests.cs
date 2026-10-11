using Xunit;
using FluentAssertions;
using Praktikum542.DTOs;
using System.Collections.Generic;

namespace Praktikum542.Tests.KazchukTests
{
    public class UnitTests
    {
        [Fact]
        public void PagedResultDto_TotalPages_CalculatesCorrectlyForPartialPage()
        {
            var paged = new PagedResultDto<string>
            {
                TotalCount = 25,
                PageSize = 10,
                Page = 1,
                Items = new List<string>()
            };

            paged.TotalPages.Should().Be(3);
        }

        [Fact]
        public void PagedResultDto_TotalPages_CalculatesCorrectlyForExactPage()
        {
            var paged = new PagedResultDto<string>
            {
                TotalCount = 20,
                PageSize = 10,
                Page = 1,
                Items = new List<string>()
            };

            paged.TotalPages.Should().Be(2);
        }

        [Fact]
        public void PagedResultDto_HasNextPage_IsTrueWhenMorePagesExist()
        {
            var paged = new PagedResultDto<string>
            {
                TotalCount = 30,
                PageSize = 10,
                Page = 2,
                Items = new List<string>()
            };

            paged.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public void PagedResultDto_HasPreviousPage_IsFalseOnFirstPage()
        {
            var paged = new PagedResultDto<string>
            {
                TotalCount = 30,
                PageSize = 10,
                Page = 1,
                Items = new List<string>()
            };

            paged.HasPreviousPage.Should().BeFalse();
        }

        [Fact]
        public void PagedResultDto_HasPreviousPage_IsTrueOnSecondPage()
        {
            var paged = new PagedResultDto<string>
            {
                TotalCount = 30,
                PageSize = 10,
                Page = 2,
                Items = new List<string>()
            };

            paged.HasPreviousPage.Should().BeTrue();
        }
    }
}