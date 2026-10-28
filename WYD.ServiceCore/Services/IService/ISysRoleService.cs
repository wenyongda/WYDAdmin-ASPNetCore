using WYD.Model.Dto;
using WYD.Model.Dto.System;
using WYD.Model.System;

namespace WYD.ServiceCore.Services.IService;

/// <summary>
/// 角色服务接口
/// </summary>
public interface ISysRoleService
{
    /// <summary>
    /// 根据条件分页查询角色数据
    /// </summary>
    Task<PageResult<SysRoleDto>> SelectRoleListAsync(SysRoleQueryDto query);

    /// <summary>
    /// 查询所有角色
    /// </summary>
    Task<List<SysRole>> SelectRoleAllAsync();

    /// <summary>
    /// 根据用户查询已分配的角色
    /// </summary>
    Task<List<SysRole>> SelectRolePermissionByUserIdAsync(Guid userId);

    /// <summary>
    /// 通过角色ID查询角色
    /// </summary>
    Task<SysRole?> SelectRoleByIdAsync(Guid roleId);

    /// <summary>
    /// 查询角色详情（包含菜单和部门数据权限）
    /// </summary>
    Task<SysRoleDto?> GetRoleDetailAsync(Guid roleId);

    /// <summary>
    /// 批量删除角色信息（软删除）
    /// </summary>
    Task<int> DeleteRoleByRoleIdAsync(Guid[] roleIds, string? updatedBy = null);

    /// <summary>
    /// 更改角色状态
    /// </summary>
    Task<int> UpdateRoleStatusAsync(SysRole roleDto, string? updatedBy = null);

    /// <summary>
    /// 校验角色权限字符是否唯一
    /// </summary>
    Task<string> CheckRoleKeyUniqueAsync(SysRole sysRole);

    /// <summary>
    /// 校验角色是否允许操作
    /// </summary>
    Task CheckRoleAllowedAsync(SysRole role);

    /// <summary>
    /// 新增保存角色信息
    /// </summary>
    Task<Guid> InsertRoleAsync(SysRole sysRole, string? createdBy = null);

    /// <summary>
    /// 通过角色ID删除角色和菜单关联
    /// </summary>
    Task<int> DeleteRoleMenuByRoleIdAsync(Guid roleId);

    /// <summary>
    /// 授权角色菜单权限（按增量同步）
    /// </summary>
    Task<bool> AuthDataScopeAsync(SysRoleDto role, string? updatedBy = null);

    /// <summary>
    /// 新增角色菜单信息
    /// </summary>
    Task<int> InsertRoleMenuAsync(SysRoleDto sysRoleDto);

    /// <summary>
    /// 判断是否是管理员
    /// </summary>
    Task<bool> IsAdminAsync(Guid userId);

    /// <summary>
    /// 获取角色菜单ID集合
    /// </summary>
    Task<List<Guid>> SelectUserRoleMenusAsync(Guid roleId);

    /// <summary>
    /// 获取角色部门ID集合
    /// </summary>
    Task<List<Guid>> SelectRoleDeptsAsync(Guid roleId);

    /// <summary>
    /// 根据角色集合获取菜单ID集合
    /// </summary>
    Task<List<Guid>> SelectRoleMenuByRoleIdsAsync(Guid[] roleIds);

    /// <summary>
    /// 获取用户角色列表
    /// </summary>
    Task<List<SysRole>> SelectUserRoleListByUserIdAsync(Guid userId);

    /// <summary>
    /// 获取用户已启用角色ID集合
    /// </summary>
    Task<List<Guid>> SelectUserRolesAsync(Guid userId);

    /// <summary>
    /// 获取用户角色权限字符集合
    /// </summary>
    Task<List<string>> SelectUserRoleKeysAsync(Guid userId);

    /// <summary>
    /// 获取用户角色名称集合
    /// </summary>
    Task<List<string>> SelectUserRoleNamesAsync(Guid userId);

    /// <summary>
    /// 修改保存角色信息
    /// </summary>
    Task<int> UpdateRoleAsync(SysRole sysRole, string? updatedBy = null);

    /// <summary>
    /// 修改角色基础信息（数据范围、备注等）
    /// </summary>
    Task<int> UpdateSysRoleAsync(SysRole sysRole, string? updatedBy = null);
}
