using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using WYD.Infrastructure.Exceptions;

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

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetClientUserIp(this HttpContext context)
        {
            if (context == null) return "";
            // 获取客户端IP地址，优先使用 X-Forwarded-For 头部（适用于反向代理场景）
            var result = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(result))
            {
                // 如果没有 X-Forwarded-For 头部，则使用 RemoteIpAddress
                result = context.Connection.RemoteIpAddress?.ToString();
            }
            if (string.IsNullOrEmpty(result))
                throw new CustomException("获取IP失败");

            if (result.Contains("::1"))
                result = "127.0.0.1";

            result = result.Replace("::ffff:", "");
            result = result.Split(':')?.FirstOrDefault() ?? "127.0.0.1";
            result = IsIP(result) ? result : "127.0.0.1";
            return result;
        }

        /// <summary>
        /// 判断是否IP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
    }
}
