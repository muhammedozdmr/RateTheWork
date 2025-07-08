using System.Security.Cryptography;
using System.Text;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Events.User;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Kullanıcı entity'si - Platformdaki kullanıcıları temsil eder.
/// Rich domain model ile iş kurallarını içerir.
/// </summary>
public class User : AuditableBaseEntity, IAggregateRoot
{
    // Constants
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 128;
    private const int MaxUsernameLength = 50;
    private const int MaxWarningsBeforeAutoBan = 3;

    // Properties - Kullanıcı Bilgileri
    public string AnonymousUsername { get; private set; } = string.Empty;
    public string HashedPassword { get; private set; } = string.Empty;
    
    // Properties - Kişisel Bilgiler (Şifreli)
    public string Email { get; private set; } = string.Empty;
    public string EncryptedFirstName { get; private set; } = string.Empty;
    public string EncryptedLastName { get; private set; } = string.Empty;
    public string Profession { get; private set; } = string.Empty;
    public string EncryptedTcIdentityNumber { get; private set; } = string.Empty;
    public string EncryptedAddress { get; private set; } = string.Empty;
    public string EncryptedCity { get; private set; } = string.Empty;
    public string EncryptedDistrict { get; private set; } = string.Empty;
    public string EncryptedPhoneNumber { get; private set; } = string.Empty;
    public string Gender { get; private set; } = string.Empty;
    public string EncryptedBirthDate { get; private set; } = string.Empty;

    // Properties - Doğrulama Durumları
    public bool IsEmailVerified { get; private set; } = false;
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiry { get; private set; }
    
    public bool IsPhoneVerified { get; private set; } = false;
    public string? PhoneVerificationCode { get; private set; }
    public DateTime? PhoneVerificationCodeExpiry { get; private set; }
    
    public bool IsTcIdentityVerified { get; private set; } = false;
    public string? TcIdentityVerificationDocumentUrl { get; private set; }

    // Properties - Platform Verileri
    public int WarningCount { get; private set; } = 0;
    public bool IsBanned { get; private set; } = false;
    public DateTime? LastLoginAt { get; private set; }
    public string? LastLoginIp { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    // Properties - Arama İndeksleri
    public string? EmailHash { get; private set; }
    public string? TcIdentityHash { get; private set; }

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private User() : base()
    {
    }

    /// <summary>
    /// Yeni kullanıcı oluşturur (Factory method)
    /// </summary>
    public static User Create(
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
        string gender,
        string encryptedBirthDate)
    {
        // Validasyonlar
        ValidateUsername(anonymousUsername);
        ValidateEmail(email);
        ValidateGender(gender);

        var user = new User
        {
            AnonymousUsername = anonymousUsername,
            HashedPassword = hashedPassword,
            Email = email.ToLowerInvariant(),
            EncryptedFirstName = encryptedFirstName,
            EncryptedLastName = encryptedLastName,
            Profession = profession,
            EncryptedTcIdentityNumber = encryptedTcIdentityNumber,
            EncryptedAddress = encryptedAddress,
            EncryptedCity = encryptedCity,
            EncryptedDistrict = encryptedDistrict,
            EncryptedPhoneNumber = encryptedPhoneNumber,
            Gender = gender,
            EncryptedBirthDate = encryptedBirthDate,
            EmailHash = GenerateHash(email.ToLowerInvariant()),
            TcIdentityHash = GenerateHash(encryptedTcIdentityNumber)
        };

        // Domain Event
        user.AddDomainEvent(new UserRegisteredEvent(
            user.Id,
            user.Email,
            user.AnonymousUsername,
            DateTime.UtcNow
        ));

        return user;
    }

    /// <summary>
    /// Email doğrulamasını tamamlar
    /// </summary>
    public void VerifyEmail()
    {
        if (IsEmailVerified)
            throw new BusinessRuleException("Email zaten doğrulanmış.");

        IsEmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiry = null;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new UserEmailVerifiedEvent(
            Id,
            Email,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Telefon doğrulamasını tamamlar
    /// </summary>
    public void VerifyPhone()
    {
        if (IsPhoneVerified)
            throw new BusinessRuleException("Telefon zaten doğrulanmış.");

        IsPhoneVerified = true;
        PhoneVerificationCode = null;
        PhoneVerificationCodeExpiry = null;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new UserPhoneVerifiedEvent(
            Id,
            EncryptedPhoneNumber, // Şifreli haliyle event'e gönderiliyor
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// TC Kimlik doğrulamasını tamamlar
    /// </summary>
    public void VerifyTcIdentity(string documentUrl)
    {
        if (IsTcIdentityVerified)
            throw new BusinessRuleException("TC Kimlik zaten doğrulanmış.");

        IsTcIdentityVerified = true;
        TcIdentityVerificationDocumentUrl = documentUrl;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new UserTcIdentityVerifiedEvent(
            Id,
            documentUrl,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Kullanıcı profilini günceller
    /// </summary>
    public void UpdateProfile(
        string? profession = null,
        string? encryptedAddress = null,
        string? encryptedCity = null,
        string? encryptedDistrict = null)
    {
        var updatedFields = new List<string>();

        if (!string.IsNullOrEmpty(profession) && profession != Profession)
        {
            Profession = profession;
            updatedFields.Add(nameof(Profession));
        }

        if (!string.IsNullOrEmpty(encryptedAddress) && encryptedAddress != EncryptedAddress)
        {
            EncryptedAddress = encryptedAddress;
            updatedFields.Add(nameof(EncryptedAddress));
        }

        if (!string.IsNullOrEmpty(encryptedCity) && encryptedCity != EncryptedCity)
        {
            EncryptedCity = encryptedCity;
            updatedFields.Add(nameof(EncryptedCity));
        }

        if (!string.IsNullOrEmpty(encryptedDistrict) && encryptedDistrict != EncryptedDistrict)
        {
            EncryptedDistrict = encryptedDistrict;
            updatedFields.Add(nameof(EncryptedDistrict));
        }

        if (updatedFields.Any())
        {
            SetModifiedDate();

            // Domain Event
            AddDomainEvent(new UserProfileUpdatedEvent(
                Id,
                updatedFields.ToArray(),
                DateTime.UtcNow
            ));
        }
    }

    /// <summary>
    /// Şifre değiştir
    /// </summary>
    public void ChangePassword(string newHashedPassword)
    {
        if (string.IsNullOrEmpty(newHashedPassword))
            throw new ArgumentNullException(nameof(newHashedPassword));

        HashedPassword = newHashedPassword;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new UserPasswordChangedEvent(
            Id,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Uyarı sayısını artırır
    /// </summary>
    public void IncrementWarningCount()
    {
        WarningCount++;
        SetModifiedDate();

        // Otomatik ban kontrolü
        if (WarningCount >= MaxWarningsBeforeAutoBan && !IsBanned)
        {
            IsBanned = true;
            // AutoBanCreatedEvent Ban entity'sinden fırlatılacak
        }
    }

    /// <summary>
    /// Kullanıcıyı banla
    /// </summary>
    public void Ban()
    {
        if (IsBanned)
            throw new BusinessRuleException("Kullanıcı zaten banlı.");

        IsBanned = true;
        SetModifiedDate();
    }

    /// <summary>
    /// Ban'ı kaldır
    /// </summary>
    public void Unban()
    {
        if (!IsBanned)
            throw new BusinessRuleException("Kullanıcı banlı değil.");

        IsBanned = false;
        SetModifiedDate();
    }

    /// <summary>
    /// Email doğrulama token'ı oluştur
    /// </summary>
    public string GenerateEmailVerificationToken()
    {
        EmailVerificationToken = Guid.NewGuid().ToString();
        EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        SetModifiedDate();
        return EmailVerificationToken;
    }

    /// <summary>
    /// Telefon doğrulama kodu oluştur
    /// </summary>
    public string GeneratePhoneVerificationCode()
    {
        var random = new Random();
        PhoneVerificationCode = random.Next(100000, 999999).ToString();
        PhoneVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
        SetModifiedDate();
        return PhoneVerificationCode;
    }

    /// <summary>
    /// Login bilgilerini güncelle
    /// </summary>
    public void UpdateLoginInfo(string ipAddress)
    {
        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
        SetModifiedDate();
    }

    /// <summary>
    /// Refresh token güncelle
    /// </summary>
    public void UpdateRefreshToken(string refreshToken, int expiryDays = 30)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiry = DateTime.UtcNow.AddDays(expiryDays);
        SetModifiedDate();
    }

    /// <summary>
    /// Soft delete override - kullanıcı silme
    /// </summary>
    public override void SoftDelete(string userId)
    {
        base.SoftDelete(userId);

        // Domain Event
        AddDomainEvent(new UserAccountDeletedEvent(
            Id,
            "User requested deletion",
            DateTime.UtcNow
        ));
    }

    // Private helper methods
    private static string GenerateHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    // Validation methods
    private static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username));

        if (username.Length > MaxUsernameLength)
            throw new BusinessRuleException($"Kullanıcı adı {MaxUsernameLength} karakterden uzun olamaz.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            throw new BusinessRuleException("Kullanıcı adı sadece harf, rakam ve alt çizgi içerebilir.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new BusinessRuleException("Geçersiz email formatı.");
    }

    private static void ValidateGender(string gender)
    {
        var validGenders = new[] { "Male", "Female", "Other", "PreferNotToSay" };
        if (!validGenders.Contains(gender))
            throw new BusinessRuleException("Geçersiz cinsiyet değeri.");
    }
}