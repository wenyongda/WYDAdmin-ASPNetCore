using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using WYD.Infrastructure.Cache;
using WYD.Infrastructure.Helper;
using WYD.Infrastructure.Localization;

namespace WYD.Infrastructure.Cache;

/// <summary>
/// 分布式缓存扩展：统一同步/异步读写、read-through 与异常降级。
///
/// 使用约定：
/// <list type="number">
/// <item>读缓存统一使用 <see cref="GetObject{T}"/> / <see cref="GetObjectAsync{T}"/> 并判断 null。
/// 不要先判断「key 是否存在」再读取 —— 那样会多一次 Redis 往返（且 GET 会取回并解码整个 value），
/// 并且 check-then-act 非原子：两次调用之间 key 可能已过期。</item>
/// <item>删除使用 <see cref="TryRemove"/> / <see cref="TryRemoveAsync"/>。
/// 不要命名为 Remove：<see cref="IDistributedCache"/> 自带实例方法 Remove(string)，
/// 实例方法优先于扩展方法，同名扩展永远不会被调用。</item>
/// <item>新代码优先使用 Async 版本，避免在请求线程上同步阻塞 Redis I/O。</item>
/// </list>
/// </summary>
public static class IDistributedCacheExtension
{
    #region Remove

    /// <summary>
    /// 删除指定 key（异常安全，失败返回 false 而不抛出）
    /// </summary>
    public static bool TryRemove(this IDistributedCache distributedCache, string key)
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
    /// 删除指定 key（异步，异常安全，失败返回 false 而不抛出）
    /// </summary>
    public static async Task<bool> TryRemoveAsync(this IDistributedCache distributedCache, string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region String

    /// <summary>
    /// 设置 string 类型的 key，包含有效时间
    /// </summary>
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
    /// 设置 string 类型的 key，包含有效时间（异步）
    /// </summary>
    public static async Task<bool> SetStringAsync(this IDistributedCache distributedCache, string key, string value,
        TimeSpan timeOut, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.SetStringAsync(key, value,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = timeOut }, cancellationToken)
                .ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Object

    /// <summary>
    /// 设置 object 类型的 key（无过期时间）
    /// </summary>
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
    /// 设置 object 类型的 key，包含有效时间
    /// </summary>
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
    /// 设置 object 类型的 key（无过期时间，异步）
    /// </summary>
    public static async Task<bool> SetObjectAsync(this IDistributedCache distributedCache, string key, object value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var valueStr = JsonHelper.ObjectToJson(value);
            // options 为必填参数；全部留空表示不过期，与同步版 SetObject(key, value) 语义一致
            await distributedCache.SetStringAsync(key, valueStr, new DistributedCacheEntryOptions(), cancellationToken)
                .ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 设置 object 类型的 key，包含有效时间（异步）
    /// </summary>
    public static async Task<bool> SetObjectAsync(this IDistributedCache distributedCache, string key, object value,
        TimeSpan timeOut, CancellationToken cancellationToken = default)
    {
        try
        {
            var valueStr = JsonHelper.ObjectToJson(value);
            await distributedCache.SetStringAsync(key, valueStr,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = timeOut }, cancellationToken)
                .ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 读取 object 类型的 key。
    /// 返回 null 表示「未命中 / key 不存在 / Redis 故障 / 内容损坏」，调用方据此回源。
    /// 注意：对值类型 T 而言，「未命中」与「缓存的值恰好是 default(T)」无法区分，
    /// 需要区分时应改用包装类型或显式存在性标记。
    /// </summary>
    public static T? GetObject<T>(this IDistributedCache distributedCache, string key)
    {
        try
        {
            var valueStr = distributedCache.GetString(key);
            if (string.IsNullOrEmpty(valueStr))
                return default;

            return JsonHelper.JsonToObject<T>(valueStr);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 读取 object 类型的 key（异步）。
    /// 返回 null 表示「未命中 / key 不存在 / Redis 故障 / 内容损坏」，调用方据此回源。
    /// </summary>
    public static async Task<T?> GetObjectAsync<T>(this IDistributedCache distributedCache, string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var valueStr = await distributedCache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(valueStr))
                return default;

            return JsonHelper.JsonToObject<T>(valueStr);
        }
        catch
        {
            return default;
        }
    }

    #endregion

    #region 旧版 ERP 兼容（StackExchange.Redis 直读 STRING）

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
    /// 直接读取 Redis STRING 类型的 key（异步，绕过 IDistributedCache 的 HASH 存储）
    /// 适用于外部系统（如旧版 ERP）以 STRING 格式写入的 key
    /// </summary>
    public static async Task<T?> GetStringAsObjectAsync<T>(this IConnectionMultiplexer multiplexer, string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = multiplexer.GetDatabase();
            var value = await db.StringGetAsync(key).ConfigureAwait(false);
            if (value.IsNull)
                return default;

            return JsonHelper.JsonToObject<T>(value!);
        }
        catch
        {
            return default;
        }
    }

    #endregion

    #region read-through

    /// <summary>
    /// 缓存优先读取：命中缓存直接返回，未命中则执行 func 并存入缓存。
    /// 读、写缓存的异常均被吞掉并视为未命中/写入失败，不影响业务主流程。
    /// </summary>
    /// <param name="cache">缓存实例；为 null 时退化为直接执行 func</param>
    /// <param name="cacheKey">完整缓存键</param>
    /// <param name="func">回源逻辑；返回 null 时不写缓存</param>
    /// <param name="cacheTime">缓存秒数；&lt;= 0 表示不过期</param>
    public static T? GetCacheObj<T>(this IDistributedCache cache, string cacheKey, Func<T> func, int cacheTime = 0)
        where T : class
    {
        var cached = cache?.GetObject<T>(cacheKey);
        if (cached != null)
            return cached;

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
    /// 缓存优先读取（异步）：命中缓存直接返回，未命中则执行 func 并存入缓存。
    /// </summary>
    public static async Task<T?> GetCacheObjAsync<T>(this IDistributedCache cache, string cacheKey,
        Func<Task<T>> func, int cacheTime = 0, CancellationToken cancellationToken = default) where T : class
    {
        T? cached = cache == null
            ? null
            : await cache.GetObjectAsync<T>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var obj = await func().ConfigureAwait(false);
        if (obj == null)
            return null;

        if (cache != null)
        {
            if (cacheTime > 0)
                await cache.SetObjectAsync(cacheKey, obj, TimeSpan.FromSeconds(cacheTime), cancellationToken)
                    .ConfigureAwait(false);
            else
                await cache.SetObjectAsync(cacheKey, obj, cancellationToken).ConfigureAwait(false);
        }

        return obj;
    }

    /// <summary>
    /// 根据前缀、扩展名、ID 构建缓存键后执行缓存优先读取
    /// </summary>
    public static T? GetCacheObj<T>(this IDistributedCache cache, string prefix, string ex, string id, Func<T> func,
        int cacheTime = 0) where T : class
    {
        var cacheKey = GetCacheKey(prefix, ex, id);
        return cache.GetCacheObj(cacheKey, func, cacheTime);
    }

    /// <summary>
    /// 根据前缀、扩展名、ID 构建缓存键后执行缓存优先读取（异步）
    /// </summary>
    public static Task<T?> GetCacheObjAsync<T>(this IDistributedCache cache, string prefix, string ex, string id,
        Func<Task<T>> func, int cacheTime = 0, CancellationToken cancellationToken = default) where T : class
    {
        var cacheKey = GetCacheKey(prefix, ex, id);
        return cache.GetCacheObjAsync(cacheKey, func, cacheTime, cancellationToken);
    }

    /// <summary>
    /// 异常安全的缓存优先读取（需 Redis 故障降级的场景，如供应商对账缓存，见 doc/缓存机制与规范.md §6.2.1）。
    /// 与 <see cref="GetCacheObj{T}(IDistributedCache, string, string, string, Func{T}, int)"/> 行为一致：
    /// <see cref="GetObject{T}"/> 内部已吞掉读异常，<see cref="SetObject"/> 内部已吞掉写异常，
    /// 因此两者等价；保留本方法用于表达「此路径必须容忍 Redis 故障」的语义。
    /// </summary>
    public static T? GetCacheObjSafe<T>(this IDistributedCache cache, string prefix, string ex, string id, Func<T> func,
        int cacheTime = 0) where T : class
    {
        var cacheKey = GetCacheKey(prefix, ex, id);
        return cache.GetCacheObj(cacheKey, func, cacheTime);
    }

    /// <summary>
    /// 异常安全的缓存优先读取（异步，需 Redis 故障降级的场景，见 doc/缓存机制与规范.md §6.2.1）
    /// </summary>
    public static Task<T?> GetCacheObjSafeAsync<T>(this IDistributedCache cache, string prefix, string ex, string id,
        Func<Task<T>> func, int cacheTime = 0, CancellationToken cancellationToken = default) where T : class
    {
        var cacheKey = GetCacheKey(prefix, ex, id);
        return cache.GetCacheObjAsync(cacheKey, func, cacheTime, cancellationToken);
    }

    #endregion

    #region Key 构建

    private static string GetCacheKey(string prefix, string ex, string id)
    {
        return $"{ex}:{prefix}:{id}:{GetCulture()}";
    }

    private static string GetCulture()
    {
        var language = CultureManage.GetCulture();
        return Culture.GetRegionCode(language);
    }

    #endregion
}
