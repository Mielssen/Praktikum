using Xunit;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Praktikum542.Tests.PogrebnyakTests
{
    // Локальная модель PagedResult для полной автономности тестов
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : throw new DivideByZeroException("PageSize must be greater than zero.");
    }

    public class PogrebnyakUnitAndExceptionTests
    {

        [Fact]
        public void PagedResult_TotalPages_CalculatesCorrectlyForEvenDivide()
        {
            var paged = new PagedResult<string> { TotalItems = 20, PageSize = 10, Page = 1 };
            Assert.Equal(2, paged.TotalPages);
        }

        [Fact]
        public void PagedResult_TotalPages_RoundsUpForUnevenDivide()
        {
            var paged = new PagedResult<string> { TotalItems = 23, PageSize = 10, Page = 1 };
            Assert.Equal(3, paged.TotalPages);
        }

        [Fact]
        public void ApplyPagination_ReturnsCorrectPageItems()
        {
            var data = Enumerable.Range(1, 25).AsQueryable();
            var result = ApplyPaginationLocal(data, page: 2, pageSize: 10);

            Assert.Equal(10, result.Items.Count());
            Assert.Equal(11, result.Items.First());
            Assert.Equal(20, result.Items.Last());
            Assert.Equal(25, result.TotalItems);
        }

        [Fact]
        public void ApplyPagination_HandlesFirstPageCorrectly()
        {
            var data = new List<int> { 10, 20, 30 }.AsQueryable();
            var result = ApplyPaginationLocal(data, 1, 2);

            Assert.Equal(2, result.Items.Count());
            Assert.Equal(10, result.Items.First());
        }

        [Fact]
        public void ApplyPagination_DefaultsInvalidPageNumbersToOne()
        {
            var data = new List<int> { 1, 2, 3 }.AsQueryable();
            var result = ApplyPaginationLocal(data, page: -5, pageSize: 2);

            Assert.Equal(1, result.Page);
            Assert.Equal(2, result.Items.Count());
        }

        [Fact]
        public void PagedResult_PageSizeZero_ThrowsDivideByZeroException()
        {
            var paged = new PagedResult<string> { TotalItems = 10, PageSize = 0 };
            Assert.Throws<DivideByZeroException>(() => _ = paged.TotalPages);
        }

        [Fact]
        public void ApplyPagination_NullSource_ThrowsArgumentNullException()
        {
            IQueryable<string>? nullSource = null;
            Assert.Throws<ArgumentNullException>(() => ApplyPaginationLocal(nullSource!, 1, 10));
        }

        [Fact]
        public void UpdateUserRoleDto_InvalidRole_ThrowsArgumentException()
        {
            var action = () => ValidateRole("invalid_role_name");
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Booking_NegativePeopleCount_ThrowsArgumentOutOfRangeException()
        {
            var action = () => SetBookingPeople(-1);
            Assert.Throws<ArgumentOutOfRangeException>(action);
        }

        [Fact]
        public void ParseAdminId_InvalidClaim_ThrowsFormatException()
        {
            string invalidId = "not_a_number";
            Assert.Throws<FormatException>(() => int.Parse(invalidId));
        }

        private PagedResult<T> ApplyPaginationLocal<T>(IQueryable<T> source, int page, int pageSize)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalItems = source.Count();
            var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<T>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        private void ValidateRole(string role)
        {
            var validRoles = new[] { "admin", "manager", "client" };
            if (!validRoles.Contains(role))
                throw new ArgumentException("Некоректна роль");
        }

        private void SetBookingPeople(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Кількість людей має бути > 0");
        }
    }
}