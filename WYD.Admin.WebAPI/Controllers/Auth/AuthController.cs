using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using WYD.Common.Extensions;
using WYD.Common.Helper;
using WYD.Infrastructure.Controllers;
using WYD.Infrastructure.CustomException;
using WYD.Infrastructure.Model;

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

    private readonly ILogger<AuthController> _logger;

    private readonly IDistributedCache _cache;

    public AuthController(ILogger<AuthController> logger, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

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

    /// <summary>
    /// 密钥交换
    /// </summary>
    [HttpPost("keyExchange")]
    public IActionResult KeyExchange([FromBody] KeyExchangeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientPublicKey))
        {
            return ToResponse(ApiResult.Error(ResultCode.PARAM_ERROR, "客户端公钥不能为空"));
        }

        if (string.IsNullOrWhiteSpace(request.TempAesKey))
        {
            return ToResponse(ApiResult.Error(ResultCode.PARAM_ERROR, "客户端临时AES密钥不能为空"));
        }

        // 从 Redis 获取服务器私钥
        var serverPrivateKey = _cache.GetObject<string>($"{RsaKeyHash}:{PrivateKeyField}");
        if (string.IsNullOrEmpty(serverPrivateKey))
        {
            return ToResponse(ApiResult.Error(ResultCode.RSA_KEY_EXPIRED, "RSA密钥已过期，请重新获取公钥"));
        }

        try
        {
            // 步骤三：使用 Server 私钥解密得到 Client 公钥
            var clientAesKey = CryptoHelper.DecryptRSA(serverPrivateKey, request.TempAesKey);

            var clientPublicKey = CryptoHelper.DecryptAes(request.ClientPublicKey, clientAesKey);

            // 步骤四：生成 AES 密钥
            var aesKey = CryptoHelper.GenerateAesKey();

            // 使用 Client 公钥加密 AES 密钥
            var encryptedAesKey = CryptoHelper.EncryptRSA(clientPublicKey, aesKey);

            // 将 AES 密钥存入 Redis，后续登录时使用
            _cache.SetObject($"{RsaKeyHash}:AesKey", aesKey, RsaKeyExpiration);

            // 返回加密后的 AES 密钥给客户端
            return OK(new KeyExchangeResponse { EncryptedAesKey = encryptedAesKey });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "密钥交换失败");
            return ToResponse(ApiResult.Error(ResultCode.KEY_EXCHANGE_ERROR, "密钥交换失败"));
        }
    }

    /// <summary>
    /// 用户登录（支持AES加密）
    /// 使用AES密钥加密的用户名密码进行登录
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] EncryptedLoginRequest request)
    {
        if (request == null) { throw new CustomException("请求参数错误"); }

        // 验证参数
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ToResponse(ApiResult.Error(ResultCode.PARAM_ERROR, "用户名和密码不能为空"));
        }

        // 从 Redis 获取 AES 密钥
        var aesKey = _cache.GetObject<string>($"{RsaKeyHash}:AesKey");
        if (string.IsNullOrEmpty(aesKey))
        {
            return ToResponse(ApiResult.Error(ResultCode.AES_KEY_EXPIRED, "AES密钥已过期，请重新进行密钥交换"));
        }

        string username, password;
        try
        {
            // 使用 AES 解密用户名和密码
            username = CryptoHelper.DecryptAes(request.Username, aesKey);
            password = CryptoHelper.DecryptAes(request.Password, aesKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AES解密失败");
            return ToResponse(ApiResult.Error(ResultCode.DECRYPT_ERROR, "数据解密失败"));
        }

        // 验证用户（密码MD5加密后比对）
        var user = await _userService.LoginAsync(username, CryptoHelper.GetMd5Hash(password));
        if (user == null)
        {
            return ToResponse(ApiResult.Error(ResultCode.LOGIN_ERROR, "用户名或密码错误"));
        }
        if (user?.Staff?.Flag?.ToStr() != "1" || user?.Flag?.ToStr() != "1")
        {
            return ToResponse(ApiResult.Error(ResultCode.LOGIN_ERROR, "帐户已停用"));
        }


        // 使用后删除 AES 密钥（一次性使用）
        _cache.Remove($"{RsaKeyHash}:AesKey");

        // 生成 JWT Token
        var jwtTokenModel = new JwtTokenModel
        {
            UserId = user.Id,
            StaffName = user.Staff.Name,
            JobNumber = user.UserName,
            JavaSessionToken = javaSessionToken ?? "",
            LoginTokenKey = loginTokenKey,
            LoginTokenValue = loginTokenValue
        };

        var accessToken = JwtUtil.GenerateJwtToken(JwtUtil.AddClaims(jwtTokenModel));

        // 生成 Refresh Token 并保存到 Redis（包含用户信息）
        var refreshToken = GuidUtils.Guid;
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);

        // 设置 httpOnly Cookie
        SetRefreshTokenCookie(refreshToken);

        // 返回结果
        return OK(new { accessToken });
    }
}

