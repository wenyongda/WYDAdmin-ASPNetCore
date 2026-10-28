namespace WYD.Model.System;

/// <summary>
/// 用户与岗位关联表 sys_user_post
/// </summary>
public class SysUserPost
{
    /// <summary>
    /// 用户ID（UUID）
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 岗位ID（UUID）
    /// </summary>
    public Guid PostId { get; set; }
}
