using System.Security.Claims;

namespace SimpleAuthentication
{
    public static class UserClaimsHelper
    {
        public static string FindFirstValue(this ClaimsPrincipal principal, string claimType)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            return principal.FindFirst(claimType)?.Value??string.Empty;

        }
        public static string GetEmail(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.FindFirstValue(ClaimTypes.Email);

        public static string GetPhoneNumber(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.FindFirstValue(ClaimTypes.MobilePhone);
        public static string GetFullName(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.FindFirstValue("FullName");
        public static string GetFirstName(this ClaimsPrincipal claimsPrincipal)
           => claimsPrincipal.FindFirstValue("FirstName");
        public static string GetLastName(this ClaimsPrincipal claimsPrincipal)
          => claimsPrincipal.FindFirstValue("LastName");
        public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.FindFirstValue("Id");
        public static string GetUserRole(this ClaimsPrincipal claimsPrincipal)
           => claimsPrincipal.FindFirstValue(ClaimTypes.Role);
        

    }

}
