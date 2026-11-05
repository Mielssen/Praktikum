using System.Security.Claims;
using Praktikum542.Exceptions;

namespace Praktikum542.Common
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out var id))
                throw new AppException("UNAUTHORIZED", "Користувач не ідентифікований");

            return id;
        }
    }
}