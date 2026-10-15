using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Serilog;
using WYD.Entity;

namespace WYD.Admin.WebAPI.Extensions
{
    /// <summary>
    /// 数据库上下文扩展方法 - 封装 DbContext 注册、初始化和预热逻辑
    /// </summary>
    public static class DatabaseContextExtensions
    {
        /// <summary>
        /// 注册所有 DbContext
        /// </summary>
        public static IServiceCollection AddDatabaseContexts(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            // 注册 AdminDbContext（PostgreSQL）
            services.AddDbContext<AdminDbContext>(options =>
            {
                options.UseNpgsql(config.GetConnectionString("DefaultConnection") ?? "", o =>
                {
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });

                options.EnableDetailedErrors();
                if (!env.IsProduction())
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            return services;
        }

        /// <summary>
        /// 初始化 DbContext 静态属性并预热
        /// </summary>
        public static void InitializeDatabaseContexts(this WebApplication app)
        {
            var config = app.Configuration;

            // 设置静态连接字符串和 Schema
            AdminDbContext.ConnectionString = config.GetConnectionString("DefaultConnection");
            AdminDbContext.DefaultSchema = config.GetSection("ConnectionStringSchemas")["DefaultConnection"] ?? "public";

            // EF Core 预热：启动时提前初始化 DbContext，防止首次请求慢
            using var scope = app.Services.CreateScope();
            var sp = scope.ServiceProvider;

            WarmUpDbContext<AdminDbContext>(sp, "AdminDbContext");
        }

        private static void WarmUpDbContext<TContext>(IServiceProvider sp, string name) where TContext : DbContext
        {
            try
            {
                var dbContext = sp.GetRequiredService<TContext>();
                _ = dbContext.Model;              // 触发 EF Core 模型编译（主要耗时项）
                dbContext.Database.CanConnect();  // 验证数据库连接、预热连接池
                Log.Information("{DbContextName} 预热完成", name);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "{DbContextName} 预热失败", name);
            }
        }
    }
}
