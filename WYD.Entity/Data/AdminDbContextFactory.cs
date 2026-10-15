using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WYD.Entity.Data
{
    /// <summary>
    /// 设计时 DbContext 工厂 - 供 EF Core 工具（migrations add/update）使用
    /// 避免工具启动整个应用程序导致失败
    /// </summary>
    public class AdminDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
    {
        public AdminDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AdminDbContext>();
            optionsBuilder.UseNpgsql("Host=192.168.6.4;Port=5432;Database=adtest;Username=postgres;Password=1");
            return new AdminDbContext(optionsBuilder.Options);
        }
    }
}
