namespace WYD.Model.System;

/// <summary>
/// 系统模块通用常量
/// </summary>
public static class UserConstants
{
    /// <summary>
    /// 校验结果：唯一
    /// </summary>
    public const string UNIQUE = "0";

    /// <summary>
    /// 校验结果：不唯一
    /// </summary>
    public const string NOT_UNIQUE = "1";

    /// <summary>
    /// 管理员角色标识
    /// </summary>
    public const string AdminRole = "admin";

    /// <summary>
    /// 管理员权限标识
    /// </summary>
    public const string AdminPerm = "*:*:*";

    /// <summary>
    /// 正常状态
    /// </summary>
    public const string NORMAL = "0";

    /// <summary>
    /// 停用状态
    /// </summary>
    public const string DISABLE = "1";

    /// <summary>
    /// 角色正常状态（角色状态为 int）
    /// </summary>
    public const int ROLE_NORMAL = 0;
}
