namespace WYD.Infrastructure.Model;

    /// <summary>
    /// API统一返回结果
    /// </summary>
public class ApiResult : Dictionary<string, object?>
{
    /// <summary>状态码</summary>
    public static readonly string CODE_TAG = "code";

    /// <summary>返回内容</summary>
    public static readonly string MSG_TAG = "msg";

    /// <summary>数据对象</summary>
    public static readonly string DATA_TAG = "data";

    public ApiResult() { }

    public ApiResult(int code, string msg)
    {
        Add(CODE_TAG, code);
        Add(MSG_TAG, msg);
    }

    public ApiResult(int code, string msg, object? data)
    {
        Add(CODE_TAG, code);
        Add(MSG_TAG, msg);
        if (data != null)
        {
            Add(DATA_TAG, data);
        }
    }

    /// <summary>返回成功消息</summary>
    public static ApiResult Ok() => new ApiResult(HttpStatus.OK, "ok");

    /// <summary>返回成功消息</summary>
    public static ApiResult Ok(object data) => new ApiResult(HttpStatus.OK, "ok", data);

    /// <summary>返回成功消息</summary>
    public static ApiResult Ok(string msg) => new ApiResult(HttpStatus.OK, msg, null);

    /// <summary>返回成功消息</summary>
    public static ApiResult Ok(string msg, object data) => new ApiResult(HttpStatus.OK, msg, data);

    public static ApiResult Error(ResultCode code, string msg = "") => Error((int)code, msg);

    /// <summary>返回失败消息</summary>
    public static ApiResult Error(int code, string msg) => new ApiResult(code, msg);

    /// <summary>返回失败消息</summary>
    public static ApiResult Error(string msg) => new ApiResult((int)ResultCode.CUSTOM_ERROR, msg);

    /// <summary>是否为成功消息</summary>
    public bool IsOk() => HttpStatus.OK == (int)this[CODE_TAG];

    /// <summary>方便链式调用</summary>
    public ApiResult Put(string key, object? value)
    {
        Add(key, value);
        return this;
    }
}

/// <summary>
/// 泛型API返回结果
/// </summary>
public class ApiResult<T> : ApiResult
{
    public T? Result { get; set; }
