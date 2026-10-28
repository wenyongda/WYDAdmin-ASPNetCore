namespace WYD.Model.System;

/// <summary>
/// 角色和部门关联表 sys_role_dept
/// </summary>
public class SysRoleDept
{
    /// <summary>
    /// 角色ID（UUID）
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 部门ID（UUID）
    /// </summary>
    public Guid DeptId { get; set; }
}
