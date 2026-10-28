namespace WYD.Infrastructure;

/// <summary>
/// JWT 配置
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// 签名密钥（长度至少 32 字节）
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 签发者
    /// </summary>
    public string Issuer { get; set; } = "WYDAdmin";

    /// <summary>
    /// 受众
    /// </summary>
    public string Audience { get; set; } = "WYDAdmin";

    /// <summary>
    /// 访问令牌有效期（分钟）
    /// </summary>
    public int Expire { get; set; } = 120;
}
