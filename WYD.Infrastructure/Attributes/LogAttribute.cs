using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace WYD.Infrastructure.Attributes
{
    /// <summary>
    /// 操作日志切面（脚手架阶段仅记录到 Serilog）
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class LogAttribute : Attribute, IAsyncActionFilter
    {
        public string Title { get; set; } = string.Empty;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sw = Stopwatch.StartNew();
            var result = await next();
            sw.Stop();

            var controller = context.Controller.GetType().Name;
            var action = context.ActionDescriptor.DisplayName;
            Serilog.Log.Information("[{Title}] {Controller}.{Action} 耗时 {ElapsedMs}ms", Title, controller, action, sw.ElapsedMilliseconds);
        }
    }
}
