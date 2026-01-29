using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Tests.Infrastructure
{
    // Class for creating a mock IHttpContextAccessor with a specified user ID
    internal static class HttpContextMock
    {
        public static IHttpContextAccessor Create(string userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext
            {
                User = principal
            };

            return new HttpContextAccessor
            {
                HttpContext = context
            };
        }
    }
}
