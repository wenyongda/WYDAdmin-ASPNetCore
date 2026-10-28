using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using UAParser;
using WYD.Infrastructure.Exceptions;
using WYD.Infrastructure.IPTools;
using WYD.Infrastructure.Model;

namespace WYD.Infrastructure.WebExtensions
{
    /// <summary>
    /// HttpContext 扩展类
    /// </summary>
    public static class HttpContextExtension
    {
        /// <summary>
        /// HttpContext.Items 中缓存的当前登录用户 Key，避免同请求内重复解析
        /// </summary>
        public const string CurrentUserCacheKey = "__CurrentUser";

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        public static string GetClientUserIp(this HttpContext context)
        {
            if (context == null) return "";

            // 优先使用 X-Forwarded-For 头部（适用于反向代理场景）
            var result = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(result))
            {
                result = context.Connection.RemoteIpAddress?.ToString();
            }
            if (string.IsNullOrEmpty(result))
                throw new CustomException("获取IP失败");

            if (result.Contains("::1"))
                result = "127.0.0.1";

            result = result.Replace("::ffff:", "");
            result = result.Split(':').FirstOrDefault() ?? "127.0.0.1";
            result = IsIP(result) ? result : "127.0.0.1";
            return result;
        }

        /// <summary>
        /// 判断是否 IP
        /// </summary>
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        /// <summary>
        /// 获取登录用户 id（UUID）
        /// </summary>
        public static Guid GetUId(this HttpContext context)
        {
            return GetCurrentUser(context)?.UserId ?? Guid.Empty;
        }

        /// <summary>
        /// 获取部门 id（UUID）
        /// </summary>
        public static Guid GetDeptId(this HttpContext context)
        {
            return GetCurrentUser(context)?.DeptId ?? Guid.Empty;
        }

        /// <summary>
        /// 获取登录用户名
        /// </summary>
        public static string GetName(this HttpContext context)
        {
            return GetCurrentUser(context)?.UserName ?? string.Empty;
        }

        /// <summary>
        /// 判断是否是管理员
        /// </summary>
        public static bool IsAdmin(this HttpContext context)
        {
            return GetCurrentUser(context)?.IsAdmin() == true;
        }

        /// <summary>
        /// 获取 ClaimsIdentity 集合
        /// </summary>
        public static IEnumerable<ClaimsIdentity>? GetClaims(this HttpContext context)
        {
            return context.User?.Identities;
        }

        /// <summary>
        /// 获取 User-Agent
        /// </summary>
        public static string GetUserAgent(this HttpContext context)
        {
            return context.Request.Headers["User-Agent"].ToString();
        }

        /// <summary>
        /// 获取请求令牌
        /// </summary>
        public static string GetToken(this HttpContext context)
        {
            return context.Request.Headers["Authorization"].ToString();
        }

        /// <summary>
        /// 获取租户 ID
        /// </summary>
        public static string GetTenantId(this HttpContext context)
        {
            return context.Request.Headers["tenantId"].ToString();
        }

        /// <summary>
        /// 获取请求 Url
        /// </summary>
        public static string GetRequestUrl(this HttpContext context)
        {
            return context?.Request.Path.Value ?? string.Empty;
        }

        /// <summary>
        /// 获取请求参数
        /// </summary>
        public static string GetQueryString(this HttpContext context)
        {
            return context?.Request.QueryString.Value ?? string.Empty;
        }

        /// <summary>
        /// 获取 body 请求参数
        /// </summary>
        public static string GetBody(this HttpContext context)
        {
            context.Request.EnableBuffering();
            context.Request.Body.Position = 0;

            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = reader.ReadToEnd();

            context.Request.Body.Position = 0;
            return body;
        }

        /// <summary>
        /// 获取 body 请求参数（异步）
        /// </summary>
        public static async Task<string> GetBodyAsync(this HttpContext context)
        {
            context.Request.EnableBuffering();
            context.Request.Body.Position = 0;

            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            context.Request.Body.Position = 0;
            return body;
        }

        /// <summary>
        /// 获取浏览器信息
        /// </summary>
        public static ClientInfo GetClientInfo(this HttpContext context)
        {
            var str = context.GetUserAgent();
            var uaParser = Parser.GetDefault();
            return uaParser.Parse(str);
        }

        /// <summary>
        /// 根据 IP 获取地理位置（未配置 IP 库时降级为未知，不影响登录）
        /// </summary>
        public static string GetIpInfo(string IP)
        {
            try
            {
                var ipInfo = IpTool.Search(IP);
                return ipInfo?.Province + "-" + ipInfo?.City + "-" + ipInfo?.NetworkOperator;
            }
            catch
            {
                return "未知-未知-";
            }
        }

        /// <summary>
        /// 获取请求参数（POST/PUT/DELETE 读取 body，否则读取 query）
        /// </summary>
        public static string GetRequestValue(this HttpContext context, string reqMethod)
        {
            string param = string.Empty;

            if (HttpMethods.IsPost(reqMethod) || HttpMethods.IsPut(reqMethod) || HttpMethods.IsDelete(reqMethod))
            {
                param = context.GetBody();
                const string regex = "(?<=\"password\":\")[^\",]*";
                param = Regex.Replace(param, regex, "***");
            }
            if (string.IsNullOrEmpty(param))
            {
                param = context.GetQueryString();
            }
            return param;
        }

        /// <summary>
        /// 获取当前用户登录信息
        /// </summary>
        public static LoginUser? GetCurrentUser(this HttpContext context)
        {
            // 优先从 HttpContext.Items 获取，避免同请求内重复解析
            if (context.Items.TryGetValue(CurrentUserCacheKey, out var cached) && cached is LoginUser cachedUser)
            {
                return cachedUser;
            }

            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            LoginUser? loginUser = null;

            // 优先使用 JWT 中的 UserData 完整用户信息
            var userData = context.User.FindFirstValue(ClaimTypes.UserData);
            if (!string.IsNullOrWhiteSpace(userData))
            {
                try
                {
                    loginUser = JsonSerializer.Deserialize<LoginUser>(userData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException)
                {
                    // UserData 不是合法 JSON 时回退到标准 Claims
                }
            }

            // 回退：从标准 Claims 组装
            loginUser ??= new LoginUser
            {
                UserId = ParseGuid(context.User.FindFirstValue(ClaimTypes.NameIdentifier)),
                DeptId = ParseGuid(context.User.FindFirstValue(ClaimTypes.GroupSid)),
                UserName = context.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                NickName = context.User.FindFirstValue("nickName") ?? string.Empty,
                RoleKeys = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                Permissions = context.User.FindAll("permission").Select(c => c.Value).ToList()
            };

            context.Items[CurrentUserCacheKey] = loginUser;
            return loginUser;
        }

        /// <summary>
        /// 是否有敏感数据权限
        /// </summary>
        public static bool HasSensitivePerm(this HttpContext context, string perm)
        {
            if (context.IsAdmin()) return true;

            var perms = context.User?.FindFirst("sensitivePerms")?.Value
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            return perms.Contains(perm);
        }

        private static Guid ParseGuid(string? value)
        {
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
