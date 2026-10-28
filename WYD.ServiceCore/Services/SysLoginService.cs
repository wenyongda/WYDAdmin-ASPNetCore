using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using WYD.Common.Helper;
using WYD.Entity;
using WYD.Infrastructure.Exceptions;
using WYD.Infrastructure.WebExtensions;
using WYD.Model.Dto;
using WYD.Model.Dto.System;
using WYD.Model.System;
using WYD.ServiceCore.Resources;
using WYD.ServiceCore.Services.IService;

namespace WYD.ServiceCore.Services;

/// <summary>
/// 登录服务
/// </summary>
public class SysLoginService : ISysLoginService
{
    private readonly AdminDbContext _adminDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISysUserService _sysUserService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public SysLoginService(
        AdminDbContext adminDbContext,
        IHttpContextAccessor httpContextAccessor,
        ISysUserService sysUserService,
        IStringLocalizer<SharedResource> localizer)
    {
        _adminDbContext = adminDbContext;
        _httpContextAccessor = httpContextAccessor;
        _sysUserService = sysUserService;
        _localizer = localizer;
    }

    /// <summary>
    /// 登录验证
    /// </summary>
    public async Task<SysUser> LoginAsync(LoginBodyDto loginBody, SysLoginInfo loginInfo)
    {
        loginInfo.UserName = loginBody.Username;
        loginInfo.Status = "1";
        loginInfo.LoginTime = DateTime.UtcNow;
        loginInfo.IpAddr = loginBody.LoginIP ?? string.Empty;
        loginInfo.ClientId = loginBody.ClientId;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var clientInfo = httpContext.GetClientInfo();
            loginInfo.Browser = clientInfo.ToString();
            loginInfo.Os = clientInfo.OS.ToString();
        }

        if (string.IsNullOrEmpty(loginBody.Password))
        {
            loginInfo.Msg = _localizer["login_pwd_error"].Value;
            await AddLoginInfoAsync(loginInfo);
            throw new CustomException(ResultCode.LOGIN_ERROR, loginInfo.Msg, false);
        }

        // 前端可能传明文，也可能传 MD5（32 位）
        if (loginBody.Password.Length != 32)
        {
            loginBody.Password = CryptoHelper.GetMd5Hash(loginBody.Password);
        }

        var user = await _sysUserService.LoginAsync(loginBody);

        if (user == null)
        {
            loginInfo.Msg = _localizer["login_pwd_error"].Value;
            await AddLoginInfoAsync(loginInfo);
            throw new CustomException(ResultCode.LOGIN_ERROR, loginInfo.Msg, false);
        }

        loginInfo.UserId = user.UserId;

        if (user.Status == "1")
        {
            loginInfo.Msg = _localizer["login_user_disabled"].Value;
            await AddLoginInfoAsync(loginInfo);
            throw new CustomException(ResultCode.LOGIN_ERROR, loginInfo.Msg, false);
        }

        loginInfo.Status = "0";
        loginInfo.Msg = _localizer["login_success"].Value;
        await AddLoginInfoAsync(loginInfo);
        await _sysUserService.UpdateLoginInfoAsync(user.UserId, loginInfo.IpAddr, loginInfo.LoginTime);

        return user;
    }

    /// <summary>
    /// 分页查询登录日志
    /// </summary>
    public async Task<PageResult<SysLoginInfo>> GetLoginLogAsync(SysLoginInfoQueryDto query)
    {
        query ??= new SysLoginInfoQueryDto();

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

        var source = _adminDbContext.SysLoginInfos.AsNoTracking();

        if (query.BeginTime == null)
        {
            // 默认查询当天（UTC）
            var today = DateTime.UtcNow.Date;
            source = source.Where(e => e.LoginTime >= today);
        }
        else
        {
            var begin = ToUtc(query.BeginTime.Value);
            source = source.Where(e => e.LoginTime >= begin);

            if (query.EndTime.HasValue)
            {
                var end = ToUtc(query.EndTime.Value);
                source = source.Where(e => e.LoginTime <= end);
            }
        }

        if (query.UserId.HasValue && query.UserId.Value != Guid.Empty)
        {
            source = source.Where(e => e.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(e => e.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.IpAddr))
        {
            source = source.Where(e => e.IpAddr == query.IpAddr);
        }

        if (!string.IsNullOrWhiteSpace(query.UserName))
        {
            source = source.Where(e => e.UserName.Contains(query.UserName));
        }

        var total = await source.CountAsync();

        var items = await source
            .OrderByDescending(e => e.LoginTime)
            .ThenByDescending(e => e.InfoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<SysLoginInfo>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 记录登录日志
    /// </summary>
    public async Task AddLoginInfoAsync(SysLoginInfo loginInfo)
    {
        _adminDbContext.SysLoginInfos.Add(loginInfo);
        await _adminDbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 清空登录日志
    /// </summary>
    public Task TruncateLoginInfoAsync()
    {
        return _adminDbContext.SysLoginInfos.ExecuteDeleteAsync();
    }

    /// <summary>
    /// 根据主键批量删除登录日志
    /// </summary>
    public Task<int> DeleteLoginInfoByIdsAsync(Guid[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return Task.FromResult(0);
        }

        return _adminDbContext.SysLoginInfos
            .Where(e => ids.Contains(e.InfoId))
            .ExecuteDeleteAsync();
    }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }
}
