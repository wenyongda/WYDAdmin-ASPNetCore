using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace WYD.Common.Helper;

/// <summary>
/// 加密工具类
/// </summary>
public static class CryptoHelper
{
    /// <summary>
    /// 获取文件的MD5值
    /// </summary>
    /// <param name="fileStream">文件流</param>
    /// <returns>MD5哈希值（小写32位）</returns>
    public static string GetMD5HashFromFile(Stream fileStream)
    {
        using var md5 = MD5.Create();
        byte[] retVal = md5.ComputeHash(fileStream);
        var sb = new StringBuilder();
        for (int i = 0; i < retVal.Length; i++)
        {
            sb.Append(retVal[i].ToString("x2"));
        }
        return sb.ToString();
    }

    /// <summary>
    /// 获取字符串的MD5值
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>MD5哈希值（大写32位）</returns>
    public static string GetMd5Hash(string input)
    {
        using MD5 md5Hash = MD5.Create();
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString().ToUpper();
    }

    /// <summary>
    /// 生成RSA PEM格式的公钥和密钥
    /// </summary>
    /// <param name="strength">密钥长度，默认2048位</param>
    /// <returns>KeyPairRSA: PublicPem=公钥, PrivatePem=私钥</returns>
    public static KeyPairRSA CreateKeyPairRSA(int strength = 2048)
    {
        var r = new RsaKeyPairGenerator();
        r.Init(new KeyGenerationParameters(new SecureRandom(), strength));
        AsymmetricCipherKeyPair keys = r.GenerateKeyPair();

        var privateTextWriter = new StringWriter();
        var privatePemWriter = new PemWriter(privateTextWriter);
        privatePemWriter.WriteObject(keys.Private);
        privatePemWriter.Writer.Flush();

        var publicTextWriter = new StringWriter();
        var publicPemWriter = new PemWriter(publicTextWriter);
        publicPemWriter.WriteObject(keys.Public);
        publicPemWriter.Writer.Flush();

        return new KeyPairRSA(publicTextWriter.ToString(), privateTextWriter.ToString());
    }

    /// <summary>
    /// RSA加密
    /// </summary>
    /// <param name="publicPem">PEM格式公钥</param>
    /// <param name="encryptStr">待加密的字符串</param>
    /// <returns>加密后的Base64字符串</returns>
    public static string EncryptRSA(string publicPem, string encryptStr)
    {
        using var reader = new StringReader(publicPem);
        var key = new PemReader(reader).ReadObject() as AsymmetricKeyParameter
            ?? throw new ArgumentException("无效的公钥格式");

        var pkcs1 = new Pkcs1Encoding(new RsaEngine());
        pkcs1.Init(true, key); // true=加密

        byte[] entData = Encoding.UTF8.GetBytes(encryptStr);
        entData = pkcs1.ProcessBlock(entData, 0, entData.Length);
        return Convert.ToBase64String(entData);
    }

    /// <summary>
    /// RSA解密
    /// </summary>
    /// <param name="privatePem">PEM格式私钥</param>
    /// <param name="decryptStr">待解密的Base64字符串</param>
    /// <returns>解密后的字符串</returns>
    public static string DecryptRSA(string privatePem, string decryptStr)
    {
        using var reader = new StringReader(privatePem);
        var obj = new PemReader(reader).ReadObject();

        AsymmetricKeyParameter key;
        if (obj is AsymmetricCipherKeyPair keyPair)
        {
            key = keyPair.Private;
        }
        else if (obj is AsymmetricKeyParameter keyParam)
        {
            key = keyParam;
        }
        else
        {
            throw new ArgumentException("无效的私钥格式");
        }

        var rsaDecrypt = new Pkcs1Encoding(new RsaEngine());
        rsaDecrypt.Init(false, key); // false=解密

        byte[] entData = Convert.FromBase64String(decryptStr);
        entData = rsaDecrypt.ProcessBlock(entData, 0, entData.Length);
        return Encoding.UTF8.GetString(entData);
    }

    /// <summary>
    /// 生成AES密钥（Base64格式）
    /// </summary>
    /// <returns>Base64编码的AES密钥</returns>
    public static string GenerateAesKey()
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        return Convert.ToBase64String(aes.Key);
    }

    /// <summary>
    /// AES加密
    /// </summary>
    /// <param name="plainText">明文</param>
    /// <param name="keyBase64">Base64编码的AES密钥</param>
    /// <returns>Base64编码的密文（IV+密文）</returns>
    public static string EncryptAes(string plainText, string keyBase64)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(keyBase64);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        // 写入IV
        ms.Write(aes.IV, 0, aes.IV.Length);
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            cs.Write(plainBytes, 0, plainBytes.Length);
            cs.FlushFinalBlock();
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// AES解密
    /// </summary>
    /// <param name="cipherTextBase64">Base64编码的密文（IV+密文）</param>
    /// <param name="keyBase64">Base64编码的AES密钥</param>
    /// <returns>明文</returns>
    public static string DecryptAes(string cipherTextBase64, string keyBase64)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);

        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(keyBase64);

        // 提取IV
        byte[] iv = new byte[16];
        Array.Copy(cipherBytes, 0, iv, 0, 16);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipherBytes, 16, cipherBytes.Length - 16);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}

/// <summary>
/// RSA密钥对
/// </summary>
/// <param name="PublicPem">PEM格式公钥</param>
/// <param name="PrivatePem">PEM格式私钥</param>
public record KeyPairRSA(string? PublicPem, string? PrivatePem);
