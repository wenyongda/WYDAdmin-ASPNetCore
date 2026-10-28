namespace WYD.Model.System;

/// <summary>
/// 用户和角色关联表 sys_user_role
/// </summary>
public class SysUserRole
{
    /// <summary>
    /// 用户ID（UUID）
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 角色ID（UUID）
    /// </summary>
    public Guid RoleId { get; set; }
}
