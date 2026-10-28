namespace WYD.Model.Dto.PhoneRebindRecord
{
    /// <summary>
    /// 电话号码换绑记录传输对象
    /// </summary>
    public class PhoneRebindRecordDto
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 旧号码
        /// </summary>
        public string OldPhone { get; set; } = string.Empty;

        /// <summary>
        /// 新号码
        /// </summary>
        public string NewPhone { get; set; } = string.Empty;

        /// <summary>
        /// 绑定应用名称
        /// </summary>
        public string AppName { get; set; } = string.Empty;

        /// <summary>
        /// 换绑时间
        /// </summary>
        public DateTime RebindTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// 新增换绑记录请求对象
    /// </summary>
    public class PhoneRebindRecordCreateDto
    {
        /// <summary>
        /// 旧号码
        /// </summary>
        public string OldPhone { get; set; } = string.Empty;

        /// <summary>
        /// 新号码
        /// </summary>
        public string NewPhone { get; set; } = string.Empty;

        /// <summary>
        /// 绑定应用名称
        /// </summary>
        public string AppName { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }

    /// <summary>
    /// 更新换绑记录请求对象
    /// </summary>
    public class PhoneRebindRecordUpdateDto
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 旧号码
        /// </summary>
        public string OldPhone { get; set; } = string.Empty;

        /// <summary>
        /// 新号码
        /// </summary>
        public string NewPhone { get; set; } = string.Empty;

        /// <summary>
        /// 绑定应用名称
        /// </summary>
        public string AppName { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }

    /// <summary>
    /// 换绑记录分页查询参数
    /// </summary>
    public class PhoneRebindRecordQueryDto
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
        /// 旧号码（模糊查询）
        /// </summary>
        public string? OldPhone { get; set; }

        /// <summary>
        /// 新号码（模糊查询）
        /// </summary>
        public string? NewPhone { get; set; }

        /// <summary>
        /// 绑定应用名称（模糊查询）
        /// </summary>
        public string? AppName { get; set; }

        /// <summary>
        /// 换绑时间起始
        /// </summary>
        public DateTime? RebindTimeStart { get; set; }

        /// <summary>
        /// 换绑时间结束
        /// </summary>
        public DateTime? RebindTimeEnd { get; set; }
    }
}
