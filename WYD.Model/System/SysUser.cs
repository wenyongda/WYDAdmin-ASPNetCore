using System.Text.Json.Serialization;
using WYD.Model.System.Enums;

namespace WYD.Model.System;

/// <summary>
/// 用户表
/// </summary>
public class SysUser : SysBase
{
    /// <summary>
    /// 用户id（UUID）
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
    /// 用户类型（00系统用户）
    /// </summary>
    public string UserType { get; set; } = "00";

    /// <summary>
    /// 头像
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 手机号
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 用户性别
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// 帐号状态（0正常 1停用）
    /// </summary>
    public string Status { get; set; } = "0";

    /// <summary>
    /// 删除标志（0存在 1已删除）
    /// </summary>
    public DelFlagEnum DelFlag { get; set; } = DelFlagEnum.Normal;

    /// <summary>
    /// 最后登录IP
    /// </summary>
    public string? LoginIP { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LoginDate { get; set; }

    /// <summary>
    /// 部门Id（UUID）
    /// </summary>
    public Guid DeptId { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// 最后密码更新时间
    /// </summary>
    public DateTime? PwdUpdateTime { get; set; }

    /// <summary>
    /// 是否管理员（UUID 主键下不再使用 UserId == 1 判断，按登录名约定）
    /// </summary>
    public bool IsAdmin => string.Equals(UserName, "admin", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 角色id集合（UUID）
    /// </summary>
    public Guid[]? RoleIds { get; set; }

    /// <summary>
    /// 岗位集合（UUID）
    /// </summary>
    public Guid[]? PostIds { get; set; }
}
