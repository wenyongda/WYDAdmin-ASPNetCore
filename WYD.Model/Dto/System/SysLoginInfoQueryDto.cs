namespace WYD.Model.Dto.System;

/// <summary>
/// 登录日志分页查询参数
/// </summary>
public class SysLoginInfoQueryDto
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
    /// 开始时间
    /// </summary>
    public DateTime? BeginTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 登录状态（0成功 1失败）
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 登录IP
    /// </summary>
    public string? IpAddr { get; set; }

    /// <summary>
    /// 用户账号
    /// </summary>
    public string? UserName { get; set; }
}
