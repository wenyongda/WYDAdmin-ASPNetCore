using Microsoft.EntityFrameworkCore;
using WYD.Entity;
using WYD.Model.Dto.System;
using WYD.Model.System;
using WYD.Model.System.Enums;
using WYD.ServiceCore.Services.IService;

namespace WYD.ServiceCore.Services;

public class SysUserService : ISysUserService
{
    private readonly AdminDbContext _adminDbContext;

    public SysUserService(AdminDbContext adminDbContext)
    {
        _adminDbContext = adminDbContext;
    }

    /// <summary>
    /// 登录校验，返回 null 表示用户不存在或密码错误
    /// </summary>
    public Task<SysUser?> LoginAsync(LoginBodyDto user)
    {
        return _adminDbContext
            .SysUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == user.Username
                && u.Password.ToLower() == user.Password.ToLower()
                && u.DelFlag == DelFlagEnum.Normal);
    }

    /// <summary>
    /// 更新最后登录 IP 和登录时间
    /// </summary>
    public Task UpdateLoginInfoAsync(Guid userId, string loginIp, DateTime loginTime)
    {
        return _adminDbContext.SysUsers
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.LoginIP, loginIp)
                .SetProperty(x => x.LoginDate, loginTime));
    }

    /// <summary>
    /// 根据用户ID查询用户
    /// </summary>
    public Task<SysUser?> GetByIdAsync(Guid userId)
    {
        return _adminDbContext.SysUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.DelFlag == DelFlagEnum.Normal);
    }
}
