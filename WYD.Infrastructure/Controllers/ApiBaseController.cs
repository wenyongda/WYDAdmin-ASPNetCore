using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Json;
using WYD.Infrastructure.Converter;
using WYD.Infrastructure.Exceptions;
using WYD.Infrastructure.Extensions;
using WYD.Infrastructure.Model;

namespace WYD.Infrastructure.Controllers
{
    /// <summary>
    /// 控制器基类，统一返回格式（JSON 使用 System.Text.Json）
    /// </summary>
    [ApiController]
    public abstract class ApiBaseController : ControllerBase
    {
        private static string TIME_FORMAT_FULL = "yyyy-MM-dd HH:mm:ss";

        protected ApiResult GetApiResult(ResultCode resultCode, object data = null)
        {
            var msg = resultCode.GetDescription();
            return new ApiResult((int)resultCode, msg, data);
        }

        protected IActionResult OK(object? data, string timeFormatStr = "yyyy-MM-dd HH:mm:ss")
        {
            var jsonStr = GetJsonStr(GetApiResult(data != null ? ResultCode.OK : ResultCode.NO_DATA, data), timeFormatStr);
            return Content(jsonStr, "application/json");
        }
        protected IActionResult OK(object? data)
        {
            var res = GetApiResult(data != null ? ResultCode.OK : ResultCode.NO_DATA, data);
            return new JsonResult(res); // 使用框架原生JsonResult，不再自己GetJsonStr
        }

        protected IActionResult ERROR(string msg, string timeFormatStr = "yyyy-MM-dd HH:mm:ss")
        {
            return ToResponse(new ApiResult((int)ResultCode.FAIL, msg, timeFormatStr));
        }

        protected IActionResult ToResponse(ApiResult apiResult, string timeFormatStr = "yyyy-MM-dd HH:mm:ss")
        {
            string jsonStr = GetJsonStr(apiResult, timeFormatStr);
            return Content(jsonStr, "application/json");
        }

        private static string GetJsonStr(ApiResult apiResult, string timeFormatStr)
        {
            if (string.IsNullOrEmpty(timeFormatStr))
            {
                timeFormatStr = TIME_FORMAT_FULL;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters = { new DateTimeFormatConverter(timeFormatStr) }
            };

            return JsonSerializer.Serialize(apiResult, options);
        }
    }
}
