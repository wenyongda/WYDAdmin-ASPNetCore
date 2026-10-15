using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace WYD.Infrastructure.WebExtensions
{
    /// <summary>
    /// HttpContext 扩展，获取当前登录用户标识
    /// </summary>
    public static class HttpContextExtension
    {
        /// <summary>
        /// 获取当前用户标识（用户名/工号），未登录返回 "system"
        /// </summary>
        public static string GetCurrentUser(this HttpContext httpContext)
        {
            var user = httpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return "system";

            return user.FindFirstValue(ClaimTypes.Name)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "system";
        }
    }
}
