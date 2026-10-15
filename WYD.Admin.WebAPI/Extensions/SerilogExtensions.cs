using Serilog;
using Serilog.Events;

namespace WYD.Admin.WebAPI.Extensions
{
    /// <summary>
    /// Serilog 日志配置扩展方法
    /// </summary>
    public static class SerilogExtensions
    {
        /// <summary>
        /// 配置并创建 Serilog Logger
        /// </summary>
        public static void CreateSerilogLogger(IConfiguration config)
        {
            var baseDir = AppContext.BaseDirectory;
            var logDir = Path.Combine(baseDir, "App_Log");

            var serilogConfig = config.GetSection("Serilog");
            var minimumLevel = serilogConfig["MinimumLevel:Default"] ?? "Debug";
            var outputTemplate = serilogConfig["OutputTemplate"]
                ?? "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {SourceContext}{NewLine}      {Message:lj}{NewLine}{Exception}";
            var consoleMinLevel = serilogConfig["Console:MinimumLevel"] ?? "Information";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(Enum.Parse<LogEventLevel>(minimumLevel))
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Map(
                    keySelector: logEvent => logEvent.Level,
                    configure: (level, sinkConfiguration) =>
                    {
                        var levelDir = level.ToString();
                        var filePath = Path.Combine(logDir, levelDir, "log-.txt");
                        sinkConfiguration.File(
                            filePath,
                            rollingInterval: RollingInterval.Day,
                            outputTemplate: outputTemplate
                        );
                    }
                )
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(logEvent => logEvent.Level == LogEventLevel.Debug)
                    .WriteTo.File(
                        path: Path.Combine(logDir, "All", "log-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: outputTemplate
                    )
                )
                .WriteTo.Console(
                    restrictedToMinimumLevel: Enum.Parse<LogEventLevel>(consoleMinLevel),
                    outputTemplate: outputTemplate)
                .CreateLogger();
        }

        /// <summary>
        /// 清除默认日志提供程序并使用 Serilog
        /// </summary>
        public static WebApplicationBuilder UseSerilogLogging(this WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog();
            return builder;
        }
    }
}
