using System;

namespace WYD.Model
{
    /// <summary>
    /// 电话号码换绑记录
    /// Table: T_PHONE_REBIND_RECORD
    /// </summary>
    public class PhoneRebindRecord
    {
        /// <summary>
        /// 主键ID（UUIDv7）
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 旧号码
        /// </summary>
        public string OldPhone { get; set; }

        /// <summary>
        /// 新号码
        /// </summary>
        public string NewPhone { get; set; }

        /// <summary>
        /// 绑定应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 换绑时间
        /// </summary>
        public DateTime RebindTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// 创建人名称
        /// </summary>
        public string CreateName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新人Id
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
        /// 是否删除（软删除标记）
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
