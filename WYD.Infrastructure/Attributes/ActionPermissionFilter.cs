using Microsoft.AspNetCore.Mvc.Filters;

namespace WYD.Infrastructure.Attributes
{
    /// <summary>
    /// 权限校验特性（脚手架阶段仅声明，不强制鉴权）
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ActionPermissionFilterAttribute : Attribute, IAsyncActionFilter
    {
        public string? Permission { get; set; }

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // TODO: 接入认证体系后在此校验 Permission
            return next();
        }
    }
}
