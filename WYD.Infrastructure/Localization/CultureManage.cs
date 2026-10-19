using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace WYD.Infrastructure.Localization;

/// <summary>
/// 多语言管理：从请求头 Accept-Language 解析当前语言，并写回响应头
/// </summary>
public static class CultureManage
{
    private const string CultureHeaderName = "Accept-Language";

    private static IHttpContextAccessor? _httpContextAccessor;

    /// <summary>
    /// 注入 IHttpContextAccessor，应用启动时调用一次
    /// </summary>
    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 获取当前请求语言，未取到或无法识别时回退到中文
    /// </summary>
    public static Culture.Language GetCulture()
    {
        foreach (var accepted in GetAcceptedLanguages())
        {
            if (ParseLanguage(accepted) is { } language)
                return language;
        }

        return Culture.Language.Chinese;
    }

    /// <summary>
    /// 将当前语言以区域代码形式写回响应头
    /// </summary>
    public static void SetCulture(Culture.Language language)
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext == null) return;

        httpContext.Response.Headers[CultureHeaderName] = Culture.GetRegionCode(language);
    }

    /// <summary>
    /// 解析语言标识：先按枚举名匹配（Chinese/English/German），再按区域代码匹配（zh-CN/en-US/de-DE）
    /// </summary>
    private static Culture.Language? ParseLanguage(string value)
    {
        if (Enum.TryParse<Culture.Language>(value, ignoreCase: true, out var language)
            && Enum.IsDefined(language))
        {
            return language;
        }

        return Culture.GetLanguage(value);
    }

    /// <summary>
    /// 解析 Accept-Language 请求头，按 q 值降序返回候选语言，例如 "zh-CN,zh;q=0.9,en;q=0.8"
    /// </summary>
    private static IEnumerable<string> GetAcceptedLanguages()
    {
        var headerValue = _httpContextAccessor?.HttpContext?.Request.Headers[CultureHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(headerValue))
            yield break;

        if (!StringWithQualityHeaderValue.TryParseList(headerValue.Split(','), out var values))
            yield break;

        foreach (var value in values.Where(v => v.Quality is null or > 0).OrderByDescending(v => v.Quality ?? 1))
        {
            var language = value.Value.ToString();
            if (!string.IsNullOrWhiteSpace(language))
                yield return language;
        }
    }
}
