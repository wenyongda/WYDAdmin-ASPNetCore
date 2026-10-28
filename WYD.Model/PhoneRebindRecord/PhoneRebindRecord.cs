using System;
using WYD.Model.System;
using WYD.Model.System.Enums;

namespace WYD.Model.PhoneRebindRecord
{
    /// <summary>
    /// 电话号码换绑记录
    /// Table: T_PHONE_REBIND_RECORD
    /// </summary>
    public class PhoneRebindRecord : SysBase
    {
        /// <summary>
        /// 主键ID（UUIDv7）
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
        /// 删除标志（0存在 1已删除）
        /// </summary>
        public DelFlagEnum DelFlag { get; set; } = DelFlagEnum.Normal;
    }
}
