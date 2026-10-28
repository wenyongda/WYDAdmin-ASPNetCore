using WYD.Model.Dto;
using WYD.Model.Dto.System;
using WYD.Model.System;

namespace WYD.ServiceCore.Services.IService;

public interface ISysLoginService
{
    /// <summary>
    /// 登录验证
    /// </summary>
    Task<SysUser> LoginAsync(LoginBodyDto loginBody, SysLoginInfo loginInfo);

    /// <summary>
    /// 分页查询登录日志
    /// </summary>
    Task<PageResult<SysLoginInfo>> GetLoginLogAsync(SysLoginInfoQueryDto query);

    /// <summary>
    /// 记录登录日志
    /// </summary>
    Task AddLoginInfoAsync(SysLoginInfo loginInfo);

    /// <summary>
    /// 清空登录日志
    /// </summary>
    Task TruncateLoginInfoAsync();

    /// <summary>
    /// 根据主键批量删除登录日志
    /// </summary>
    Task<int> DeleteLoginInfoByIdsAsync(Guid[] ids);
}
