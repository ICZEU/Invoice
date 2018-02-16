using System.Security.Claims;

namespace ICZEU.Invoice.WebApp.Extensions
{
    public static class UserClaimExtensions
    {
        public static string GetName(this ClaimsPrincipal user)
        {
            return user.FindFirst("name")?.Value ?? user.Identity.Name;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value;
        }
    }
}
