namespace WYD.Infrastructure
{
    /// <summary>
    /// 全局静态常量
    /// </summary>
    public static class GlobalConstant
    {
        /// <summary>
        /// 管理员权限
        /// </summary>
        public const string AdminPerm = "*:*:*";

        /// <summary>
        /// 管理员角色
        /// </summary>
        public const string AdminRole = "admin";

        /// <summary>
        /// 用户权限缓存 key 前缀
        /// </summary>
        public const string UserPermKEY = "CACHE-USER-PERM_";
    }
}
