using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WYD.Infrastructure.Helper;

public static class JsonHelper
{
    /// <summary>
    /// 通过 Key 获取 Value
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <param name="key">要获取的键名</param>
    /// <returns>键对应的值，不存在时返回 null</returns>
    public static string? GetValueByKey(string json, string key)
    {
        using var doc = JsonDocument.Parse(json);
        var jsonElement = doc.RootElement.Clone();

        return jsonElement.GetProperty(key).GetString();
    }


    /// <summary> 
    /// 对象 转 Json
    /// </summary> 
    /// <param name="obj">对象</param> 
    /// <returns>JSON格式的字符串</returns> 
    public static string ObjectToJson(object obj)
    {
        JsonSerializerOptions options = new()
        {
            //关闭默认转义
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //启用驼峰格式
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //关闭缩进设置
            WriteIndented = false
        };

        return JsonSerializer.Serialize(obj, options);
    }


    /// <summary> 
    /// Json 转 对象
    /// </summary> 
    /// <typeparam name="T">目标类型</typeparam> 
    /// <param name="json">JSON 字符串</param> 
    /// <returns>指定类型的对象</returns> 
    public static T JsonToObject<T>(string json)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };//启用大小写不敏感

        return JsonSerializer.Deserialize<T>(json, options)!;
    }


    /// <summary>
    /// 解析 JSON 字符串为 JsonNode
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <returns>JsonNode 对象，解析失败时返回 null</returns>
    public static JsonNode? ParseJsonNode(string json)
    {
        var jsonNode = JsonNode.Parse(json);

        return jsonNode;
    }

}