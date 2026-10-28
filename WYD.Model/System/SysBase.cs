namespace WYD.Model.System
{
    /// <summary>
    /// 实体基类：统一承载审计字段与备注。
    /// 继承该类的实体，其字段映射由 WYD.Entity 中的统一约定完成，
    /// 效果类似 SqlSugar 在基类上标注 [SugarColumn]。
    /// </summary>
    public abstract class SysBase
    {
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateBy { get; set; } = string.Empty;

        /// <summary>
        /// 创建人名称
        /// </summary>
        public string CreateName { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新人
        /// </summary>
        public string? UpdateBy { get; set; }

        /// <summary>
        /// 更新人名称
        /// </summary>
        public string? UpdateName { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
