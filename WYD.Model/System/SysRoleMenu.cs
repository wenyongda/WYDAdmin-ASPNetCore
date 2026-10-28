namespace WYD.Model.System;

/// <summary>
/// 角色和菜单关联表 sys_role_menu
/// </summary>
public class SysRoleMenu
{
    /// <summary>
    /// 角色ID（UUID）
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 菜单ID（UUID）
    /// </summary>
    public Guid MenuId { get; set; }
}
