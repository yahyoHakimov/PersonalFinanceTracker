using PersonalFinanceTracker.Domain.Enums;
using System.Security.Claims;

namespace PersonalFinanceTracker.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        public static UserRole GetRole(this ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.User;
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.GetRole() == UserRole.Admin;
        }

        public static bool IsUser(this ClaimsPrincipal user)
        {
            return user.GetRole() == UserRole.User;
        }
    }
}
