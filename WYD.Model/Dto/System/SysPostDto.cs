namespace WYD.Model.Dto.System;

/// <summary>
/// 岗位传输对象
/// </summary>
public class SysPostDto
{
    /// <summary>
    /// 岗位ID（UUID）
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// 岗位编码
    /// </summary>
    public string PostCode { get; set; } = string.Empty;

    /// <summary>
    /// 岗位名称
    /// </summary>
    public string PostName { get; set; } = string.Empty;

    /// <summary>
    /// 显示顺序
    /// </summary>
    public int PostSort { get; set; }

    /// <summary>
    /// 状态（0正常 1停用）
    /// </summary>
    public string Status { get; set; } = "0";

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

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
/// 岗位分页查询参数
/// </summary>
public class SysPostQueryDto
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
    /// 岗位编码（模糊查询）
    /// </summary>
    public string? PostCode { get; set; }

    /// <summary>
    /// 岗位名称（模糊查询）
    /// </summary>
    public string? PostName { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string? Status { get; set; }
}
