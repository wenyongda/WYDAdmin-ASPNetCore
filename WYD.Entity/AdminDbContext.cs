using Microsoft.EntityFrameworkCore;
using WYD.Entity.Mappings;
using WYD.Model.PhoneRebindRecord;
using WYD.Model.System;

namespace WYD.Entity
{
    /// <summary>
    /// Admin 数据库上下文
    /// </summary>
    public class AdminDbContext : DbContext
    {
        /// <summary>
        /// 静态连接字符串（供扩展方法设置）
        /// </summary>
        public static string? ConnectionString { get; set; }

        /// <summary>
        /// 默认数据库 Schema
        /// </summary>
        public static string? DefaultSchema { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseNpgsql(ConnectionString);
        }

        public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
        {
        }

        #region System 模块

        public DbSet<SysUser> SysUsers { get; set; }

        public DbSet<SysLoginInfo> SysLoginInfos { get; set; }

        public DbSet<SysConfig> SysConfigs { get; set; }

        public DbSet<SysRole> SysRoles { get; set; }

        public DbSet<SysPost> SysPosts { get; set; }

        public DbSet<SysMenu> SysMenus { get; set; }

        /// <summary>
        /// 用户与角色关联
        /// </summary>
        public DbSet<SysUserRole> SysUserRoles { get; set; }

        /// <summary>
        /// 角色与菜单关联
        /// </summary>
        public DbSet<SysRoleMenu> SysRoleMenus { get; set; }

        /// <summary>
        /// 角色与部门关联（数据权限）
        /// </summary>
        public DbSet<SysRoleDept> SysRoleDepts { get; set; }

        /// <summary>
        /// 用户与岗位关联
        /// </summary>
        public DbSet<SysUserPost> SysUserPosts { get; set; }

        #endregion

        /// <summary>
        /// 电话号码换绑记录
        /// </summary>
        public DbSet<PhoneRebindRecord> PhoneRebindRecords => Set<PhoneRebindRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 应用所有实体配置
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdminDbContext).Assembly);

            // 统一配置继承 SysBase 的实体公共字段（审计字段、备注）
            modelBuilder.ApplySysBaseMapping();
        }
    }
}
