using System.Security.Claims;

namespace SimpleAuthentication
{
    internal static class UserClaimsHelper
    {
        internal static string FindFirstValue(this ClaimsPrincipal principal, string claimType)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            return principal.FindFirst(claimType)?.Value??string.Empty;

        }
        internal static string GetEmail(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.FindFirstValue(ClaimTypes.Email);

        internal static string GetPhoneNumber(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.FindFirstValue(ClaimTypes.MobilePhone);
        internal static string GetFullName(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.FindFirstValue("FullName");
        internal static string GetFirstName(this ClaimsPrincipal claimsPrincipal)
           => claimsPrincipal.FindFirstValue("FirstName");
        internal static string GetLastName(this ClaimsPrincipal claimsPrincipal)
          => claimsPrincipal.FindFirstValue("LastName");
        internal static string GetUserId(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.FindFirstValue("Id");
        internal static string GetUserRole(this ClaimsPrincipal claimsPrincipal)
           => claimsPrincipal.FindFirstValue(ClaimTypes.Role);
        

    }

}
