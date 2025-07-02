using System.Security.Cryptography;
using System.Text;
using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

// Kullanıcı: Platformdaki anonim kullanıcıyı temsil eder.
public class User : AuditableBaseEntity
{
   // ✅ Plain - Anonim veriler
    public string AnonymousUsername { get; set; }
    public string Email { get; set; } // Giriş için plain gerekli
    public string HashedPassword { get; set; } // BCrypt hash
    public string Profession { get; set; }
    public string Gender { get; set; }
    public string? LastCompanyWorked { get; set; }

    // 🔐 Encrypted - Hassas kişisel veriler (Azure Key Vault)
    public string EncryptedFirstName { get; set; }
    public string EncryptedLastName { get; set; }
    public string EncryptedTcIdentityNumber { get; set; }
    public string EncryptedBirthDate { get; set; } // DateTime -> string
    public string EncryptedAddress { get; set; }
    public string EncryptedCity { get; set; }
    public string EncryptedDistrict { get; set; }
    public string EncryptedPhoneNumber { get; set; }

    // ✅ Plain - Doğrulama alanları
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }

    public bool IsPhoneVerified { get; set; } = false;
    public string? PhoneVerificationCode { get; set; }
    public DateTime? PhoneVerificationCodeExpiry { get; set; }

    public bool IsTcIdentityVerified { get; set; } = false;
    public string? TcIdentityVerificationDocumentUrl { get; set; }

    // ✅ Plain - Platform verileri
    public int WarningCount { get; set; } = 0;
    public bool IsBanned { get; set; } = false;

    // 🔍 Arama için hash'ler (opsiyonel - performans için)
    public string? EmailHash { get; set; } // SHA-256 hash (arama için)
    public string? TcIdentityHash { get; set; } // Duplicate kontrolü için

    public User(
        string anonymousUsername,
        string hashedPassword,
        string email,
        string encryptedFirstName,
        string encryptedLastName,
        string profession,
        string encryptedTcIdentityNumber,
        string encryptedAddress,
        string encryptedCity,
        string encryptedDistrict,
        string encryptedPhoneNumber,
        string gender
        , string encryptedBirthDate
    )
    {
        AnonymousUsername = anonymousUsername;
        HashedPassword = hashedPassword;
        Email = email;
        EncryptedFirstName = encryptedFirstName;
        EncryptedLastName = encryptedLastName;
        Profession = profession;
        EncryptedTcIdentityNumber = encryptedTcIdentityNumber;
        EncryptedAddress = encryptedAddress;
        EncryptedCity = encryptedCity;
        EncryptedDistrict = encryptedDistrict;
        EncryptedPhoneNumber = encryptedPhoneNumber;
        Gender = gender;
        EncryptedBirthDate = encryptedBirthDate;

        // Hash'leri oluştur (performans için)
        EmailHash = GenerateHash(email);
        TcIdentityHash = GenerateHash(encryptedTcIdentityNumber);
    }

    private static string GenerateHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes);
    }
}
