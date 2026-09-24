using System.Security.Cryptography;
using System.Text;

namespace SchemaNote.Services;

public interface ICryptoService
{
    /// <summary>將明文以 AES-128 加密，回傳 Base64(IV + 密文)。</summary>
    string Encrypt(string plainText);

    /// <summary>將 Base64(IV + 密文) 解密回明文。</summary>
    string Decrypt(string cipherText);

    /// <summary>嘗試解密，失敗時回傳 false。</summary>
    bool TryDecrypt(string cipherText, out string plainText);
}

/// <summary>
/// 使用 AES-128 (CBC + PKCS7) 進行對稱加解密。
/// 輸出格式為 Base64(IV(16 bytes) + CipherText)，每次加密使用隨機 IV。
/// </summary>
public class AesCryptoService : ICryptoService
{
    private const int KeySizeBits = 128;
    private const int IvSizeBytes = 16;
    private readonly byte[] _key;

    public AesCryptoService(IConfiguration configuration)
    {
        string? keyValue = configuration["Encryption:AesKey"];
        if (string.IsNullOrWhiteSpace(keyValue))
        {
            throw new InvalidOperationException("缺少組態 Encryption:AesKey，無法初始化 AES 加密服務。");
        }

        _key = ResolveKey(keyValue);
        if (_key.Length != KeySizeBits / 8)
        {
            throw new InvalidOperationException($"Encryption:AesKey 必須為 16 位元組 (AES-128)，目前為 {_key.Length} 位元組。");
        }
    }

    // 金鑰可為 Base64 字串或一般文字（以 UTF-8 取前 16 位元組並在不足時補零）。
    private static byte[] ResolveKey(string keyValue)
    {
        try
        {
            byte[] decoded = Convert.FromBase64String(keyValue);
            if (decoded.Length == KeySizeBits / 8)
            {
                return decoded;
            }
        }
        catch (FormatException)
        {
            // 非 Base64，改以 UTF-8 處理。
        }

        byte[] raw = Encoding.UTF8.GetBytes(keyValue);
        byte[] key = new byte[KeySizeBits / 8];
        Array.Copy(raw, key, Math.Min(raw.Length, key.Length));
        return key;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        using Aes aes = Aes.Create();
        aes.KeySize = KeySizeBits;
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        byte[] cipher = aes.EncryptCbc(Encoding.UTF8.GetBytes(plainText), aes.IV, PaddingMode.PKCS7);

        byte[] result = new byte[IvSizeBytes + cipher.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, IvSizeBytes);
        Buffer.BlockCopy(cipher, 0, result, IvSizeBytes, cipher.Length);
        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return string.Empty;
        }

        byte[] data = Convert.FromBase64String(cipherText);
        if (data.Length <= IvSizeBytes)
        {
            throw new CryptographicException("密文長度不足，無法解密。");
        }

        byte[] iv = new byte[IvSizeBytes];
        Buffer.BlockCopy(data, 0, iv, 0, IvSizeBytes);

        byte[] cipher = new byte[data.Length - IvSizeBytes];
        Buffer.BlockCopy(data, IvSizeBytes, cipher, 0, cipher.Length);

        using Aes aes = Aes.Create();
        aes.KeySize = KeySizeBits;
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        byte[] plain = aes.DecryptCbc(cipher, iv, PaddingMode.PKCS7);
        return Encoding.UTF8.GetString(plain);
    }

    public bool TryDecrypt(string cipherText, out string plainText)
    {
        try
        {
            plainText = Decrypt(cipherText);
            return true;
        }
        catch
        {
            plainText = string.Empty;
            return false;
        }
    }
}
