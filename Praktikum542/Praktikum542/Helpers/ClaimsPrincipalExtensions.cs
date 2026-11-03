using System.Security.Claims;
using Praktikum542.Exceptions;

namespace Praktikum542.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetCredentialId(this ClaimsPrincipal user)
        {
            var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(value, out var id))
                throw new AppException(ErrorCodes.Unauthorized, "Не вдалося визначити користувача");

            return id;
        }
    }
}