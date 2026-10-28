using Microsoft.EntityFrameworkCore;
using WYD.Entity;
using WYD.Model.Dto.System;
using WYD.Model.System;
using WYD.Model.System.Enums;
using WYD.ServiceCore.Services.IService;

namespace WYD.ServiceCore.Services;

/// <summary>
/// 角色权限服务
/// </summary>
public class SysPermissionService : ISysPermissionService
{
    private readonly ISysRoleService _roleService;
    private readonly AdminDbContext _dbContext;

    public SysPermissionService(ISysRoleService roleService, AdminDbContext dbContext)
    {
        _roleService = roleService;
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<List<string>> GetRolePermissionAsync(SysUserDto user)
    {
        if (user == null)
        {
            return new List<string>();
        }

        // 管理员拥有所有角色权限标识
        if (user.IsAdmin)
        {
            return new List<string> { UserConstants.AdminRole };
        }

        return await _roleService.SelectUserRoleKeysAsync(user.UserId);
    }

    /// <inheritdoc />
    public async Task<List<string>> GetMenuPermissionAsync(SysUserDto user)
    {
        if (user == null)
        {
            return new List<string>();
        }

        // 管理员拥有所有菜单权限
        if (user.IsAdmin)
        {
            return new List<string> { UserConstants.AdminPerm };
        }

        var roleKeys = await _roleService.SelectUserRoleKeysAsync(user.UserId);
        if (roleKeys.Any(IsAdminRole))
        {
            return new List<string> { UserConstants.AdminPerm };
        }

        // 菜单 -> 角色菜单 -> 用户角色 -> 角色，取启用角色下启用菜单的权限字符
        return await (from menu in _dbContext.SysMenus.AsNoTracking()
                      join roleMenu in _dbContext.SysRoleMenus on menu.MenuId equals roleMenu.MenuId
                      join userRole in _dbContext.SysUserRoles on roleMenu.RoleId equals userRole.RoleId
                      join role in _dbContext.SysRoles on userRole.RoleId equals role.RoleId
                      where userRole.UserId == user.UserId
                            && menu.Status == UserConstants.NORMAL
                            && role.Status == UserConstants.ROLE_NORMAL
                            && role.DelFlag == DelFlagEnum.Normal
                            && menu.Perms != null
                            && menu.Perms != ""
                      select menu.Perms!)
            .Distinct()
            .ToListAsync();
    }

    private static bool IsAdminRole(string roleKey)
    {
        return string.Equals(roleKey, UserConstants.AdminRole, StringComparison.OrdinalIgnoreCase);
    }
}
