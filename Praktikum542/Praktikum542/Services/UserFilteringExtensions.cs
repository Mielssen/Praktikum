using Praktikum542.Models;

namespace Praktikum542.Services
{
    public static class UserFilteringExtensions
    {
        public static IQueryable<Credential> ApplySearch(this IQueryable<Credential> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var searchTerm = search.Trim().ToLower();

            return query.Where(c => 
                c.Email.ToLower().Contains(searchTerm) || 
                (c.UserDetail != null && c.UserDetail.Name.ToLower().Contains(searchTerm))
            );
        }
    }
}