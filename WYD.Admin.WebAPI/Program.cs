using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using WYD.Admin.WebAPI.Extensions;
using WYD.Infrastructure;
using WYD.Infrastructure.Captcha;
using WYD.Infrastructure.Localization;

// 配置 Serilog（在 CreateBuilder 之前，确保启动日志也能捕获）
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();
SerilogExtensions.CreateSerilogLogger(config);

try
{
    Log.Information("启动 WYD.Admin.WebAPI...");

    var builder = WebApplication.CreateBuilder(args);

    // 初始化静态配置访问器（AppSettings / JwtUtil 依赖）
    _ = new AppSettings(builder.Configuration);

    // 使用 Serilog（清除默认日志提供程序）
    builder.UseSerilogLogging();

    // EF Core + PostgreSQL（注册 DbContext）
    builder.Services.AddDatabaseContexts(builder.Configuration, builder.Environment);

    // 注册应用程序服务（Scrutor 自动扫描 + Mapperly 映射器 + HttpContextAccessor）
    builder.Services.AddApplicationServices();

    // 分布式缓存（验证码、刷新令牌）
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddScoped<ICaptchaProvider, SvgCaptchaProvider>();

    // JWT 认证
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = JwtUtil.GetValidationParameters();
        });
    builder.Services.AddAuthorization();

    // 多语言资源（WYD.ServiceCore/Resources/SharedResource.resx）
    builder.Services.AddLocalization();

    // 配置控制器（JSON 使用 System.Text.Json 驼峰命名）
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

    // 配置 OpenAPI（.NET 10 内置 + Scalar UI）
    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info.Title = "WYDAdmin API";
            document.Info.Version = "1.0";
            document.Info.Description = "WYDAdmin Web API";
            return Task.CompletedTask;
        });
    });

    // 注册健康检查服务
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // 按 Accept-Language 设置当前请求的 Culture/UICulture，供 IStringLocalizer 使用
    var supportedCultures = new[] { "zh-CN", "en-US", "de-DE" };
    app.UseRequestLocalization(new RequestLocalizationOptions()
        .SetDefaultCulture("zh-CN")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures));

    // 注入 HttpContextAccessor，供多语言（CultureManage）解析 Accept-Language
    CultureManage.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

    // EF Core 预热：启动时提前初始化 DbContext，防止首次请求慢
    app.InitializeDatabaseContexts();

    // 配置 Scalar UI（仅开发环境）
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi("/openapi/v1.json");
        app.MapScalarApiReference(options =>
        {
            options.Title = "WYDAdmin API";
            options.Theme = ScalarTheme.BluePlanet;
        });
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // 优雅关闭
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStopping.Register(() =>
    {
        Log.Information("接收到停止信号，正在优雅关闭...");
    });

    lifetime.ApplicationStopped.Register(() =>
    {
        Log.Information("应用程序已完全停止");
        Log.CloseAndFlush();
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用程序启动失败");
}
finally
{
    Log.CloseAndFlush();
}
