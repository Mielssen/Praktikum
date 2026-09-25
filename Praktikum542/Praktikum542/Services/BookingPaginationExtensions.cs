using System;
using System.Collections.Generic;
using System.Linq;

namespace Praktikum542.Services
{
    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<T> Items { get; set; } = new List<T>();
    }

    public static class BookingPaginationExtensions
    {
        public static PagedResult<T> ApplyPagination<T>(this IEnumerable<T> query, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var list = query.ToList();
            int totalItems = list.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return new PagedResult<T>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = items
            };
        }
    }
}