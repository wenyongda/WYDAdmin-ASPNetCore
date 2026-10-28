namespace WYD.Model.System;

/// <summary>
/// 岗位表 sys_post
/// </summary>
public class SysPost : SysBase
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
    public string Status { get; set; } = UserConstants.NORMAL;
}
