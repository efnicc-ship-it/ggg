using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Infrastructure.Services.Encryption;

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:AesKey"]
            ?? throw new InvalidOperationException("Encryption:AesKey is not configured. Set it via environment variable.");

        // Key must be 32 bytes for AES-256
        _key = DeriveKey(keyString, 32);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = EncryptBytes(plainBytes);
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = DecryptBytes(cipherBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public byte[] EncryptBytes(byte[] data)
    {
        // Format: [12-byte nonce][16-byte tag][ciphertext]
        using var aesGcm = new AesGcm(_key, 16);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[data.Length];
        var tag = new byte[16];

        aesGcm.Encrypt(nonce, data, ciphertext, tag);

        var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length);

        return result;
    }

    public byte[] DecryptBytes(byte[] data)
    {
        // Format: [12-byte nonce][16-byte tag][ciphertext]
        using var aesGcm = new AesGcm(_key, 16);

        var nonce = new byte[12];
        var tag = new byte[16];
        var ciphertext = new byte[data.Length - nonce.Length - tag.Length];

        Buffer.BlockCopy(data, 0, nonce, 0, nonce.Length);
        Buffer.BlockCopy(data, nonce.Length, tag, 0, tag.Length);
        Buffer.BlockCopy(data, nonce.Length + tag.Length, ciphertext, 0, ciphertext.Length);

        var plaintext = new byte[ciphertext.Length];
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

        return plaintext;
    }

    private static byte[] DeriveKey(string keyString, int keyLength)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(keyString));
        var key = new byte[keyLength];
        Array.Copy(hash, key, Math.Min(hash.Length, keyLength));
        return key;
    }
}
