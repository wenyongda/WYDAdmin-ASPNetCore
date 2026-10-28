using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using WYD.Infrastructure;
using WYD.Infrastructure.Attributes;
using WYD.Infrastructure.Captcha;
using WYD.Infrastructure.Controllers;
using WYD.Infrastructure.Exceptions;
using WYD.Infrastructure.Model;
using WYD.Infrastructure.WebExtensions;
using WYD.Model.Dto.System;
using WYD.Model.System;
using WYD.ServiceCore.Services.IService;

namespace WYD.Admin.WebAPI.Controllers.System;

/// <summary>
/// 登录认证
/// </summary>
public class SysLoginController : ApiBaseController
{
    /// <summary>
    /// Refresh Token Cookie 名称
    /// </summary>
    private const string RefreshTokenCookieName = "refresh_token";

    /// <summary>
    /// Refresh Token 有效期（天）
    /// </summary>
    private const int RefreshTokenExpirationDays = 7;

    private readonly IDistributedCache _cache;
    private readonly ISysConfigService _sysConfigService;
    private readonly ISysLoginService _sysLoginService;
    private readonly ISysPermissionService _permissionService;
    private readonly ICaptchaProvider _captchaProvider;

    public SysLoginController(
        IDistributedCache cache,
        ISysConfigService sysConfigService,
        ISysLoginService sysLoginService,
        ISysPermissionService permissionService,
        ICaptchaProvider captchaProvider)
    {
        _cache = cache;
        _sysConfigService = sysConfigService;
        _sysLoginService = sysLoginService;
        _permissionService = permissionService;
        _captchaProvider = captchaProvider;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [Log(Title = "登录")]
    public async Task<IActionResult> Login([FromBody] LoginBodyDto loginBody)
    {
        if (loginBody == null)
        {
            throw new CustomException("请求参数错误");
        }

        var sysConfig = await _sysConfigService.GetSysConfigByKeyAsync("sys.account.captchaOnOff");
        if (sysConfig == null)
        {
            return ToResponse(new ApiResult((int)ResultCode.CUSTOM_ERROR, "验证码配置缺失"));
        }

        if (sysConfig.ConfigValue != "off" && !_captchaProvider.Validate(loginBody.Uuid, loginBody.Code))
        {
            return ToResponse(ResultCode.CAPTCHA_ERROR, "验证码错误");
        }

        var loginIp = string.IsNullOrWhiteSpace(loginBody.LoginIP)
            ? HttpContext.GetClientUserIp()
            : loginBody.LoginIP;
        var location = HttpContextExtension.GetIpInfo(loginIp);

        var user = await _sysLoginService.LoginAsync(loginBody,
            new SysLoginInfo { LoginLocation = location });

        var userDto = new SysUserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            NickName = user.NickName,
            DeptId = user.DeptId
        };

        var roleKeys = await _permissionService.GetRolePermissionAsync(userDto);
        var permissions = await _permissionService.GetMenuPermissionAsync(userDto);
        var jwtSettings = JwtUtil.GetSettings();

        var loginUser = new LoginUser
        {
            UserId = user.UserId,
            UserName = user.UserName,
            NickName = user.NickName,
            DeptId = user.DeptId,
            SessionId = Guid.NewGuid().ToString("N"),
            RoleKeys = roleKeys,
            Roles = new List<Roles>(),
            Permissions = permissions,
            ExpireTime = DateTime.UtcNow.AddMinutes(jwtSettings.Expire)
        };

        var accessToken = JwtUtil.GenerateJwtToken(loginUser);
        var refreshToken = await CreateRefreshTokenAsync(loginUser);
        SetRefreshTokenCookie(refreshToken);

        return OK(new
        {
            accessToken,
            tokenType = "Bearer",
            expiresIn = jwtSettings.Expire * 60
        });
    }

    /// <summary>
    /// 刷新访问令牌
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [Log(Title = "刷新令牌")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return ToResponse(new ApiResult((int)ResultCode.DENY, "刷新令牌不存在"));
        }

        var cacheKey = GetRefreshTokenCacheKey(refreshToken);
        var json = await _cache.GetStringAsync(cacheKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return ToResponse(new ApiResult((int)ResultCode.DENY, "刷新令牌已过期"));
        }

        var loginUser = JsonSerializer.Deserialize<LoginUser>(json);
        if (loginUser == null)
        {
            return ToResponse(new ApiResult((int)ResultCode.DENY, "刷新令牌无效"));
        }

        // 旋转刷新令牌，旧令牌立即失效
        await _cache.RemoveAsync(cacheKey);
        loginUser.SessionId = Guid.NewGuid().ToString("N");
        loginUser.ExpireTime = DateTime.UtcNow.AddMinutes(JwtUtil.GetSettings().Expire);

        var accessToken = JwtUtil.GenerateJwtToken(loginUser);
        var newRefreshToken = await CreateRefreshTokenAsync(loginUser);
        SetRefreshTokenCookie(newRefreshToken);

        return OK(new
        {
            accessToken,
            tokenType = "Bearer",
            expiresIn = JwtUtil.GetSettings().Expire * 60
        });
    }

    /// <summary>
    /// 退出登录
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [Log(Title = "退出登录")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _cache.RemoveAsync(GetRefreshTokenCacheKey(refreshToken));
        }

        Response.Cookies.Delete(RefreshTokenCookieName);
        return ToResponse(ApiResult.Ok("退出成功"));
    }

    private async Task<string> CreateRefreshTokenAsync(LoginUser loginUser)
    {
        var refreshToken = Guid.NewGuid().ToString("N");
        await _cache.SetStringAsync(
            GetRefreshTokenCacheKey(refreshToken),
            JsonSerializer.Serialize(loginUser),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(RefreshTokenExpirationDays)
            });

        return refreshToken;
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(RefreshTokenExpirationDays)
        });
    }

    private static string GetRefreshTokenCacheKey(string refreshToken) => $"refresh_token:{refreshToken}";
}
