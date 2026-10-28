namespace WYD.Model.Dto.System;

/// <summary>
/// 用户权限计算所需的用户信息
/// </summary>
public class SysUserDto
{
    /// <summary>
    /// 用户ID（UUID）
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 登录用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称
    /// </summary>
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 部门ID（UUID）
    /// </summary>
    public Guid DeptId { get; set; }

    /// <summary>
    /// 是否管理员（按登录名约定）
    /// </summary>
    public bool IsAdmin => string.Equals(UserName, "admin", StringComparison.OrdinalIgnoreCase);
}
