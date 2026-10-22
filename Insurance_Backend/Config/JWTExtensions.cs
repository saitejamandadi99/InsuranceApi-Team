using System.Security.Claims;

namespace Insurance_Backend.Config
{
    public static class JWTExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst("userId")!.Value);
        }
    }
}
