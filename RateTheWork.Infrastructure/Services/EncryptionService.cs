using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _iv;
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var encryptionKey = configuration["Security:Encryption:Key"] ??
                            throw new InvalidOperationException("Encryption key not configured");

        // Ensure key is 32 bytes (256 bits) for AES-256
        _key = DeriveKey(encryptionKey, 32);

        // Generate IV from key for consistency (though unique IV per message is more secure)
        _iv = DeriveKey(encryptionKey + "_IV", 16);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherText);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            // Log error but don't expose details
            return string.Empty;
        }
    }

    public void SecureDelete(ref string data)
    {
        if (string.IsNullOrEmpty(data))
            return;

        // Overwrite the string in memory
        unsafe
        {
            fixed (char* ptr = data)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    ptr[i] = '\0';
                }
            }
        }

        data = null!;
    }

    private static byte[] DeriveKey(string password, int keyBytes)
    {
        const int iterations = 10000;
        var salt = Encoding.UTF8.GetBytes("RateTheWork_Salt_2024");

        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return deriveBytes.GetBytes(keyBytes);
    }
}
