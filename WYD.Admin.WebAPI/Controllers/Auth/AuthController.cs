using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using WYD.Infrastructure.Controllers;

namespace WYD.Admin.WebAPI.Controllers.Auth;

public class AuthController : ApiBaseController
{
    private const string RsaKeyHash = "RSA_Keys";
    private const string PublicKeyField = "PublicKey";
    private const string PrivateKeyField = "PrivateKey";
    private const int RsaKeyExpirationSeconds = 300; // 5分钟过期
    private static readonly TimeSpan RsaKeyExpiration = TimeSpan.FromSeconds(RsaKeyExpirationSeconds);

    /// <summary>
    /// Refresh Token Cookie 名称
    /// </summary>
    private const string RefreshTokenCookieName = "refresh_token";

    /// <summary>
    /// Refresh Token 有效期（天）
    /// </summary>
    private const int RefreshTokenExpirationDays = 7;

    private readonly IDistributedCache _cache;



    /// <summary>
    /// 获取RSA公钥
    /// 客户端向服务器申请RSA公钥
    /// </summary>
    [HttpGet("getPublicKey")]
    public IActionResult GetPublicKey()
    {
        // 生成服务器 RSA 密钥对
        var serverKeyPair = CryptoHelper.CreateKeyPairRSA();

        // 将密钥对存入 Redis，设置过期时间
        _cache.SetObject($"{RsaKeyHash}:{PublicKeyField}", serverKeyPair.PublicPem, RsaKeyExpiration);
        _cache.SetObject($"{RsaKeyHash}:{PrivateKeyField}", serverKeyPair.PrivatePem, RsaKeyExpiration);

        // 返回公钥给客户端
        return OK(new { publicKey = serverKeyPair.PublicPem });
    }


}

