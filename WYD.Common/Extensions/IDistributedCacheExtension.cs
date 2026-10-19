using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using WYD.Common.Extensions;
using WYD.Common.Helper;
using WYD.Infrastructure.Localization;

namespace WYD.Common.Extensions;

/// <summary>
/// 扩展分布式缓存接口,集成常用方法
/// </summary>
public static class IDistributedCacheExtension
{
    /// <summary>
    /// 删除指定key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool Remove(this IDistributedCache distributedCache, string key)
    {
        try
        {
            distributedCache.Remove(key);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 设置object类型的key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool SetObject(this IDistributedCache distributedCache, string key, object value)
    {
        try
        {
            var valueStr = JsonHelper.ObjectToJson(value);
            distributedCache.SetString(key, valueStr);
            return true;
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// 设置string类型key,包含有效时间
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public static bool SetString(this IDistributedCache distributedCache, string key, string value, TimeSpan timeOut)
    {
        try
        {
            distributedCache.SetString(key, value, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = timeOut });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 设置object类型key,包含有效时间
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public static bool SetObject(this IDistributedCache distributedCache, string key, object value, TimeSpan timeOut)
    {
        try
        {
            var valueStr = JsonHelper.ObjectToJson(value);
            distributedCache.SetString(key, valueStr, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = timeOut });
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 读取 Object 类型的key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public static T GetObject<T>(this IDistributedCache distributedCache, string key)
    {
        try
        {
            var valueStr = distributedCache.GetString(key);
            var value = JsonHelper.JsonToObject<T>(valueStr);
            return value;
        }
        catch
        {
            return default!;
        }
    }

    /// <summary>
    /// 直接读取 Redis STRING 类型的 key（绕过 IDistributedCache 的 HASH 存储）
    /// 适用于外部系统（如旧版 ERP）以 STRING 格式写入的 key
    /// </summary>
    public static T? GetStringAsObject<T>(this IConnectionMultiplexer multiplexer, string key)
    {
        try
        {
            var db = multiplexer.GetDatabase();
            var value = db.StringGet(key);
            if (value.IsNull)
                return default;
            return JsonHelper.JsonToObject<T>(value!);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 判断是否存在指定key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool IsContainKey(this IDistributedCache distributedCache, string key)
    {
        if (string.IsNullOrEmpty(distributedCache.GetString(key)))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 缓存优先读取：命中缓存直接返回，未命中则执行 func 并存入缓存
    /// </summary>
    public static T? GetCacheObj<T>(this IDistributedCache cache, string cacheKey, Func<T> func, int cacheTime = 0) where T : class
    {
        if (cache != null && cache.IsContainKey(cacheKey))
            return cache.GetObject<T>(cacheKey);

        var obj = func();
        if (obj == null)
            return null;

        if (cacheTime > 0)
            cache?.SetObject(cacheKey, obj, TimeSpan.FromSeconds(cacheTime));
        else
            cache?.SetObject(cacheKey, obj);

        return obj;
    }

    /// <summary>
    /// 根据前缀、扩展名、ID 构建缓存键后执行缓存优先读取
    /// </summary>
    public static T? GetCacheObj<T>(this IDistributedCache cache, string prefix, string ex, string id, Func<T> func, int cacheTime = 0) where T : class
    {
        var cacheKey = GetCacheKey(prefix, ex, id);
        return cache.GetCacheObj(cacheKey, func, cacheTime);
    }

    /// <summary>
    /// 异常安全的缓存优先读取（需 Redis 故障降级的场景，如供应商对账缓存，见 doc/缓存机制与规范.md §6.2.1）：
    /// 与 GetCacheObj(prefix, ex, id, func) 行为一致，差异仅在 Redis 异常时不抛出 ——
    /// 读取异常视为未命中回源 DB；写缓存异常静默跳过；func 返回 null 不写缓存。
    /// </summary>
    public static T? GetCacheObjSafe<T>(this IDistributedCache cache, string prefix, string ex, string id, Func<T> func, int cacheTime = 0) where T : class
    {
        var cacheKey = GetCacheKey(prefix, ex, id);
        try
        {
            var valueStr = cache.GetString(cacheKey);
            if (!string.IsNullOrEmpty(valueStr))
            {
                var cached = JsonHelper.JsonToObject<T>(valueStr);
                if (cached != null)
                    return cached;
            }
        }
        catch
        {
            // Redis 故障：降级回源（静默，与既有扩展方法风格一致）
        }

        var obj = func();
        if (obj == null)
            return null;

        // 写缓存失败不阻断业务（SetObject 内部已吞异常返回 false）
        if (cacheTime > 0)
            cache.SetObject(cacheKey, obj, TimeSpan.FromSeconds(cacheTime));
        else
            cache.SetObject(cacheKey, obj);

        return obj;
    }

    private static string GetCacheKey(string prefix, string ex, string id)
    {
        return $"{ex}:{prefix}:{id}:{GetCulture()}";
    }

    private static string GetCulture()
    {
        var language = CultureManage.GetCulture();
        return Culture.GetRegionCode(language);
    }
}