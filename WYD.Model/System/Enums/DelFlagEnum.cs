using System.ComponentModel;

namespace WYD.Model.System.Enums
{
    /// <summary>
    /// 逻辑删除标志。
    /// 数据库列使用 int 类型：0=存在，1=已删除。
    /// </summary>
    public enum DelFlagEnum
    {
        /// <summary>
        /// 存在（未删除）
        /// </summary>
        [Description("存在")]
        Normal = 0,

        /// <summary>
        /// 已删除
        /// </summary>
        [Description("已删除")]
        Deleted = 1
    }
}
