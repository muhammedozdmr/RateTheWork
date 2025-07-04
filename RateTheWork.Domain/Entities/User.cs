using System.Security.Cryptography;
using System.Text;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Kullanıcı entity'si - Platformdaki anonim kullanıcıyı temsil eder.
/// AuditableBaseEntity'den türer ve soft delete destekler.
/// </summary>
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

    /// <summary>
    /// Yeni kullanıcı oluşturur
    /// </summary>
    public User
    (
        string anonymousUsername
        , string hashedPassword
        , string email
        , string encryptedFirstName
        , string encryptedLastName
        , string profession
        , string encryptedTcIdentityNumber
        , string encryptedAddress
        , string encryptedCity
        , string encryptedDistrict
        , string encryptedPhoneNumber
        , string gender
        , string encryptedBirthDate
    ) : base()
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

        // Domain Event ekle
        AddDomainEvent(new UserRegisteredEvent(
            Id,
            Email,
            AnonymousUsername,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Email doğrulamasını tamamlar
    /// </summary>
    public void VerifyEmail()
    {
        IsEmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiry = null;
        SetModifiedDate();
    }

    /// <summary>
    /// Telefon doğrulamasını tamamlar
    /// </summary>
    public void VerifyPhone()
    {
        IsPhoneVerified = true;
        PhoneVerificationCode = null;
        PhoneVerificationCodeExpiry = null;
        SetModifiedDate();
    }

    /// <summary>
    /// TC kimlik doğrulamasını tamamlar
    /// </summary>
    public void VerifyTcIdentity(string documentUrl)
    {
        IsTcIdentityVerified = true;
        TcIdentityVerificationDocumentUrl = documentUrl;
        SetModifiedDate();
    }

    /// <summary>
    /// Kullanıcıya uyarı ekler
    /// </summary>
    public void AddWarning()
    {
        WarningCount++;
        SetModifiedDate();
    }

    /// <summary>
    /// Kullanıcıyı banlar
    /// </summary>
    public void Ban()
    {
        IsBanned = true;
        SetModifiedDate();
    }

    /// <summary>
    /// Kullanıcının banını kaldırır
    /// </summary>
    public void Unban()
    {
        IsBanned = false;
        SetModifiedDate();
    }

    /// <summary>
    /// SHA-256 hash oluşturur
    /// </summary>
    private static string GenerateHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes);
    }
}