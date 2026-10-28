using WYD.Model.Dto.System;

namespace WYD.ServiceCore.Services.IService;

/// <summary>
/// 角色权限服务接口
/// </summary>
public interface ISysPermissionService
{
    /// <summary>
    /// 获取角色数据权限
    /// </summary>
    Task<List<string>> GetRolePermissionAsync(SysUserDto user);

    /// <summary>
    /// 获取菜单数据权限
    /// </summary>
    Task<List<string>> GetMenuPermissionAsync(SysUserDto user);
}
