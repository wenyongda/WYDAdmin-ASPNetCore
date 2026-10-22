using Microsoft.EntityFrameworkCore;
using WYD.Model;
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

        public DbSet<SysConfig> SysConfigs { get; set; }

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
        }
    }
}
