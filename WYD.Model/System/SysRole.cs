using WYD.Model.System.Enums;

namespace WYD.Model.System;

/// <summary>
/// 角色表 sys_role
/// </summary>
public class SysRole : SysBase
{
    /// <summary>
    /// 角色ID（UUID）
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// 角色权限字符
    /// </summary>
    public string RoleKey { get; set; } = string.Empty;

    /// <summary>
    /// 角色排序
    /// </summary>
    public int RoleSort { get; set; }

    /// <summary>
    /// 帐号状态（0正常 1停用）
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 删除标志（0存在 1已删除）
    /// </summary>
    public DelFlagEnum DelFlag { get; set; } = DelFlagEnum.Normal;

    /// <summary>
    /// 数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限）
    /// </summary>
    public int DataScope { get; set; } = 1;

    /// <summary>
    /// 菜单树选择项是否关联显示
    /// </summary>
    public bool MenuCheckStrictly { get; set; } = true;

    /// <summary>
    /// 部门树选择项是否关联显示
    /// </summary>
    public bool DeptCheckStrictly { get; set; } = true;

    /// <summary>
    /// 菜单组（非数据库字段）
    /// </summary>
    public Guid[]? MenuIds { get; set; }

    /// <summary>
    /// 部门组（数据权限，非数据库字段）
    /// </summary>
    public Guid[]? DeptIds { get; set; }

    /// <summary>
    /// 用户个数（非数据库字段）
    /// </summary>
    public int UserNum { get; set; }
}
