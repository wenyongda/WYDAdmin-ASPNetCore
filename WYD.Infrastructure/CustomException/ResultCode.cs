using System.ComponentModel;

namespace WYD.Infrastructure.CustomException;
/// <summary>
/// API返回结果状态码
/// </summary>
public enum ResultCode
{
    [Description("ok")]
    OK = 200,

    [Description("没有更多数据")]
    NO_DATA = 210,

    [Description("参数错误")]
    PARAM_ERROR = 101,

    [Description("验证码错误")]
    CAPTCHA_ERROR = 103,

    [Description("登录错误")]
    LOGIN_ERROR = 105,

    [Description("操作失败")]
    FAIL = 1,

    [Description("服务端出错啦")]
    GLOBAL_ERROR = 500,

    [Description("自定义异常")]
    CUSTOM_ERROR = 110,

    [Description("非法请求")]
    INVALID_REQUEST = 116,

    [Description("授权失败")]
    OAUTH_FAIL = 201,

    [Description("未授权")]
    DENY = 401,

    [Description("授权访问失败")]
    FORBIDDEN = 403,

    [Description("Bad Request")]
    BAD_REQUEST = 400,

    [Description("RSA密钥已过期")]
    RSA_KEY_EXPIRED = 1001,

    [Description("AES密钥已过期")]
    AES_KEY_EXPIRED = 1002,

    [Description("密钥交换失败")]
    KEY_EXCHANGE_ERROR = 1003,

    [Description("数据解密失败")]
    DECRYPT_ERROR = 1004
}
