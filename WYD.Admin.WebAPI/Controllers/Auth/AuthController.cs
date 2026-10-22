using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security;
using WYD.Infrastructure.Extensions;
using WYD.Common.Helper;
using WYD.Infrastructure.Controllers;
using WYD.Infrastructure.Exceptions;
using WYD.Infrastructure.Model;
using WYD.Infrastructure.WebExtensions;
using WYD.Model.Dto.System;
using WYD.ServiceCore.Services.IService;

namespace WYD.Admin.WebAPI.Controllers.Auth;

public class AuthController : ApiBaseController
{

    /// <summary>
    /// Refresh Token Cookie 名称
    /// </summary>
    private const string RefreshTokenCookieName = "refresh_token";

    /// <summary>
    /// Refresh Token 有效期（天）
    /// </summary>
    private const int RefreshTokenExpirationDays = 7;

    private readonly ILogger<AuthController> _logger;

    private readonly IDistributedCache _cache;

    private readonly ISysConfigService _sysConfigService;

    public AuthController(ILogger<AuthController> logger, IDistributedCache cache, ISysConfigService sysConfigService)
    {
        _logger = logger;
        _cache = cache;
        _sysConfigService = sysConfigService;
    }

    /// <summary>
    /// 用户登录（支持AES加密）
    /// 使用AES密钥加密的用户名密码进行登录
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginBodyDto loginBody)
    {
        if (loginBody == null) { throw new CustomException("请求参数错误"); }

        var sysConfig = _sysConfigService.GetSysConfigByKey("sys.account.captchaOnOff");
        if (sysConfig?.ConfigValue != "off" && !captchaProvider.Validate(loginBody.Uuid, loginBody.Code))
        {
            return ToResponse(ResultCode.CAPTCHA_ERROR, "验证码错误");
        }

        string location = HttpContextExtension.GetIpInfo(loginBody.LoginIP);
        var user = sysLoginService.Login(loginBody, new SysLogininfor() { LoginLocation = location });

        //TODO 异地登录

        //TODO 角色 权限

        //var loginUser = new()
        //{
        //    Permissions
        //}

        var token = JwtUtil.GenerateJwtToken(JwtUtil.AddClaims(loginUser));

        // 设置 httpOnly Cookie
        SetRefreshTokenCookie(refreshToken);

        // 返回结果
        return OK(new { accessToken });
    }
}

