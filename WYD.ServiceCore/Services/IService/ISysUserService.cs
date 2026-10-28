using WYD.Model.Dto.System;
using WYD.Model.System;

namespace WYD.ServiceCore.Services.IService;

public interface ISysUserService
{
    /// <summary>
    /// 登录校验，返回 null 表示用户不存在或密码错误
    /// </summary>
    Task<SysUser?> LoginAsync(LoginBodyDto user);

    /// <summary>
    /// 更新最后登录 IP 和登录时间
    /// </summary>
    Task UpdateLoginInfoAsync(Guid userId, string loginIp, DateTime loginTime);

    /// <summary>
    /// 根据用户ID查询用户
    /// </summary>
    Task<SysUser?> GetByIdAsync(Guid userId);
}
