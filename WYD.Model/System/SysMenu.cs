namespace WYD.Model.System;

/// <summary>
/// 系统菜单表 sys_menu
/// 当前仅映射角色权限计算所需的字段，后续迁移菜单管理时可继续扩展。
/// </summary>
public class SysMenu
{
    /// <summary>
    /// 菜单ID（UUID）
    /// </summary>
    public Guid MenuId { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string MenuName { get; set; } = string.Empty;

    /// <summary>
    /// 父菜单ID（UUID）
    /// </summary>
    public Guid ParentId { get; set; }

    /// <summary>
    /// 显示顺序
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 类型（M目录 C菜单 F按钮 L链接）
    /// </summary>
    public string MenuType { get; set; } = string.Empty;

    /// <summary>
    /// 显示状态（0显示 1隐藏）
    /// </summary>
    public string Visible { get; set; } = UserConstants.NORMAL;

    /// <summary>
    /// 菜单状态（0正常 1停用）
    /// </summary>
    public string Status { get; set; } = UserConstants.NORMAL;

    /// <summary>
    /// 权限字符串
    /// </summary>
    public string? Perms { get; set; }

    /// <summary>
    /// 菜单图标
    /// </summary>
    public string? Icon { get; set; }
}
