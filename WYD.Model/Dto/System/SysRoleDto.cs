namespace WYD.Model.Dto.System;

/// <summary>
/// 角色传输对象
/// </summary>
public class SysRoleDto
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
    /// 数据范围
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
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 菜单ID集合
    /// </summary>
    public List<Guid> MenuIds { get; set; } = new();

    /// <summary>
    /// 部门ID集合（数据权限）
    /// </summary>
    public List<Guid> DeptIds { get; set; } = new();

    /// <summary>
    /// 需要移除的菜单ID集合
    /// </summary>
    public List<Guid> DelMenuIds { get; set; } = new();

    /// <summary>
    /// 用户个数
    /// </summary>
    public int UserNum { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreateBy { get; set; } = string.Empty;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdateBy { get; set; }
}

/// <summary>
/// 角色分页查询参数
/// </summary>
public class SysRoleQueryDto
{
    /// <summary>
    /// 页码（默认1）
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页条数（默认20）
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 角色名称（模糊查询）
    /// </summary>
    public string? RoleName { get; set; }

    /// <summary>
    /// 角色权限字符
    /// </summary>
    public string? RoleKey { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// 创建时间起始
    /// </summary>
    public DateTime? BeginTime { get; set; }

    /// <summary>
    /// 创建时间结束
    /// </summary>
    public DateTime? EndTime { get; set; }
}
