using cpcApi.Model;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace cpcApi.Filter
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
                var tokenVersion = context.User.FindFirst("SessionVersion")?.Value;

                if (username != null && tokenVersion != null)
                {
                    var user = await userManager.FindByNameAsync(username);

                    if (user == null || user.SessionVersion.ToString() != tokenVersion)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Session expired");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
