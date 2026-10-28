namespace WYD.Model.System;

/// <summary>
/// 登录日志表
/// Table: sys_login_info
/// </summary>
public class SysLoginInfo
{
    /// <summary>
    /// 日志主键（UUID）
    /// </summary>
    public Guid InfoId { get; set; }

    /// <summary>
    /// 用户账号
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 登录状态（0成功 1失败）
    /// </summary>
    public string Status { get; set; } = "0";

    /// <summary>
    /// 登录IP地址
    /// </summary>
    public string IpAddr { get; set; } = string.Empty;

    /// <summary>
    /// 登录地点
    /// </summary>
    public string LoginLocation { get; set; } = string.Empty;

    /// <summary>
    /// 浏览器类型
    /// </summary>
    public string Browser { get; set; } = string.Empty;

    /// <summary>
    /// 操作系统
    /// </summary>
    public string Os { get; set; } = string.Empty;

    /// <summary>
    /// 提示消息
    /// </summary>
    public string Msg { get; set; } = string.Empty;

    /// <summary>
    /// 访问时间
    /// </summary>
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 查询开始时间（非数据库字段）
    /// </summary>
    public DateTime? BeginTime { get; set; }

    /// <summary>
    /// 查询结束时间（非数据库字段）
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 客户端id
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// 用户id
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 登录地点（仅展示用，非数据库字段）
    /// </summary>
    public string Position
    {
        get
        {
            var tempLocation = LoginLocation.Split('-')[0];
            return tempLocation == "0" ? "IP未知" : tempLocation;
        }
    }
}
