namespace WYD.Infrastructure.Localization;

/// <summary>
/// 语言与区域代码（BCP-47）映射
/// </summary>
public static class Culture
{
    /// <summary>
    /// 支持的语言
    /// </summary>
    public enum Language
    {
        Chinese = 0,
        English = 1,
        German = 2
    }

    /// <summary>
    /// 语言 → 区域代码
    /// </summary>
    public static string GetRegionCode(Language language)
    {
        return language switch
        {
            Language.Chinese => "zh-CN",
            Language.English => "en-US",
            Language.German => "de-DE",
            _ => "zh-CN"
        };
    }

    /// <summary>
    /// 区域代码 → 语言，无法识别返回 null（大小写不敏感，同时接受 zh / en / de 这类主语言子标签）
    /// </summary>
    public static Language? GetLanguage(string regionCode)
    {
        if (string.IsNullOrWhiteSpace(regionCode)) return null;

        return regionCode.Trim().ToLowerInvariant() switch
        {
            "zh" or "zh-cn" => Language.Chinese,
            "en" or "en-us" => Language.English,
            "de" or "de-de" => Language.German,
            _ => null
        };
    }
}
