using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using WYD.Infrastructure.Model;

namespace WYD.Infrastructure;

/// <summary>
/// JWT 工具类
/// </summary>
public static class JwtUtil
{
    static JwtUtil()
    {
        // 保留 ClaimTypes 原始类型，避免 UserData 等自定义声明被自动映射
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
    }

    /// <summary>
    /// 读取 JWT 配置
    /// </summary>
    public static JwtSettings GetSettings()
    {
        var settings = AppSettings.Bind("JwtSettings", new JwtSettings());
        if (string.IsNullOrWhiteSpace(settings.SecretKey))
        {
            throw new InvalidOperationException("JwtSettings:SecretKey 未配置");
        }

        return settings;
    }

    /// <summary>
    /// 生成访问令牌
    /// </summary>
    public static string GenerateJwtToken(LoginUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var settings = GetSettings();
        var now = DateTime.UtcNow;
        var expireMinutes = settings.Expire <= 0 ? 120 : settings.Expire;

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: AddClaims(user),
            notBefore: now,
            expires: now.AddMinutes(expireMinutes),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 组装 Claims
    /// </summary>
    public static List<Claim> AddClaims(LoginUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.GroupSid, user.DeptId.ToString()),
            new("nickName", user.NickName ?? string.Empty),
            new("sessionId", user.SessionId ?? string.Empty),
            new(ClaimTypes.UserData, JsonSerializer.Serialize(user))
        };

        foreach (var roleKey in user.RoleKeys ?? new List<string>())
        {
            claims.Add(new Claim(ClaimTypes.Role, roleKey));
        }

        foreach (var permission in user.Permissions ?? new List<string>())
        {
            claims.Add(new Claim("permission", permission));
        }

        return claims;
    }

    /// <summary>
    /// 令牌校验参数
    /// </summary>
    public static TokenValidationParameters GetValidationParameters()
    {
        var settings = GetSettings();

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    }
}
