using System.Reflection;
using Mapster;
using MapsterMapper;

namespace WYD.Admin.WebAPI.Extensions
{
    /// <summary>
    /// 依赖注入扩展方法 - 封装服务注册和 Scrutor 配置
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// 注册应用程序服务（Scrutor 自动扫描 + Mapster + HttpContextAccessor）
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Scrutor 自动扫描注册（从 WYD.ServiceCore 程序集）
            services.Scan(scan => scan
                .FromAssemblies(
                    Assembly.Load("WYD.Service"), Assembly.Load("WYD.ServiceCore"))
                .AddClasses()
                .AsMatchingInterface()
                .WithScopedLifetime()
            );

            // 注册 Mapster DI
            services.AddSingleton(TypeAdapterConfig.GlobalSettings);
            services.AddScoped<IMapper, ServiceMapper>();

            // 注册 HttpContextAccessor
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
