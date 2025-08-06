using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.User;
using RateTheWork.Domain.Events.User;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Common;
using RateTheWork.Domain.ValueObjects.Blockchain;

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

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private User() : base()
    {
    }

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
    public Gender Gender { get; private set; } = Gender.PreferNotToSay;
    public string EncryptedBirthDate { get; private set; } = string.Empty;

    // Properties - Doğrulama Durumları
    public bool IsEmailVerified { get; private set; } = false;
    public string? EmailVerificationToken { get; set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? EmailVerificationTokenExpiry { get; set; }

    public bool IsPhoneVerified { get; private set; } = false;
    public string? PhoneVerificationCode { get; private set; }
    public DateTime? PhoneVerificationCodeExpiry { get; private set; }

    public bool IsTcIdentityVerified { get; private set; } = false;
    public string? TcIdentityVerificationDocumentUrl { get; private set; }

    // Computed property - en az bir doğrulama yapılmış mı?
    public bool IsVerified => IsEmailVerified || IsPhoneVerified || IsTcIdentityVerified || IsBlockchainVerified;

    // Properties - Platform Verileri
    public int WarningCount { get; private set; } = 0;
    public bool IsBanned { get; private set; } = false;
    public DateTime? LastLoginAt { get; private set; }
    public string? LastLoginIp { get; private set; }
    public string? RefreshToken { get; private set; }
    
    // Application katmanı uyumluluğu için ek property'ler
    public List<JobAlert> JobAlerts { get; private set; } = new();
    public DateTime? RefreshTokenExpiry { get; private set; }

    // Properties - Arama İndeksleri
    public string? EmailHash { get; private set; }
    public string? TcIdentityHash { get; private set; }
    
    // Properties - Blockchain
    public string? BlockchainWalletAddress { get; private set; }
    public string? EncryptedBlockchainPrivateKey { get; private set; }
    public string? BlockchainPublicKey { get; private set; }
    public string? BlockchainIdentityContractAddress { get; private set; }
    public bool IsBlockchainVerified { get; private set; } = false;
    public DateTime? BlockchainVerifiedAt { get; private set; }
    
    // Application katmanı uyumluluğu için ek property'ler
    public List<string> Roles { get; private set; } = new List<string> { "User" };
    public List<string> UserRoles => Roles; // Alias
    public string FullName => $"{EncryptedFirstName} {EncryptedLastName}"; // Computed
    public string PhoneNumber => EncryptedPhoneNumber; // Alias

    /// <summary>
    /// Yeni kullanıcı oluşturur (Factory method)
    /// </summary>
    public static User Create
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
    )
    {
        // Validasyonlar
        ValidateUsername(anonymousUsername);
        ValidateEmail(email);

        // Parse gender string to enum
        if (!Enum.TryParse<Gender>(gender, true, out var genderEnum))
        {
            throw new BusinessRuleException($"Geçersiz cinsiyet değeri: {gender}");
        }

        var user = new User
        {
            AnonymousUsername = anonymousUsername, HashedPassword = hashedPassword, Email = email.ToLowerInvariant()
            , EncryptedFirstName = encryptedFirstName, EncryptedLastName = encryptedLastName, Profession = profession
            , EncryptedTcIdentityNumber = encryptedTcIdentityNumber, EncryptedAddress = encryptedAddress
            , EncryptedCity = encryptedCity, EncryptedDistrict = encryptedDistrict
            , EncryptedPhoneNumber = encryptedPhoneNumber, Gender = genderEnum, EncryptedBirthDate = encryptedBirthDate
            , EmailHash = GenerateHash(email.ToLowerInvariant())
            , TcIdentityHash = GenerateHash(encryptedTcIdentityNumber)
            , IsBlockchainVerified = false
        };

        // Domain Event
        user.AddDomainEvent(new UserRegisteredEvent(
            user.Id,
            user.Email,
            user.AnonymousUsername,
            "0.0.0.0", // RegisterIp - will be set by the application layer
            "Unknown", // UserAgent - will be set by the application layer
            null, // ReferrerUrl
            "Web", // RegistrationSource
            user.CreatedAt
        ));

        return user;
    }

    /// <summary>
    /// Test için basitleştirilmiş kullanıcı oluşturur
    /// </summary>
    public static User CreateForTesting(string email, string username)
    {
        var user = new User
        {
            Email = email.ToLowerInvariant(), AnonymousUsername = username, HashedPassword = "test-hash"
            , EncryptedFirstName = "encrypted-test", EncryptedLastName = "encrypted-test"
            , EncryptedTcIdentityNumber = "encrypted-12345678901", EncryptedAddress = "encrypted-test-address"
            , EncryptedCity = "encrypted-istanbul", EncryptedDistrict = "encrypted-kadikoy"
            , EncryptedPhoneNumber = "encrypted-5551234567", Gender = Gender.PreferNotToSay
            , EncryptedBirthDate = "encrypted-2000-01-01", Profession = "Test"
            , EmailHash = GenerateHash(email.ToLowerInvariant()), TcIdentityHash = GenerateHash("encrypted-12345678901")
        };

        return user;
    }

    //TODO: Bu factory metodu yapmayı unutma eventini de yap 
    // /// <summary>
    // /// CSV/Excel import'tan kullanıcı oluşturur
    // /// </summary>
    // public static User CreateFromImport(Dictionary<string, string> importData, IEncryptionService encryptionService)
    // {
    //     // Import verilerini validate et
    //     if (!importData.ContainsKey("email") || !importData.ContainsKey("tcno"))
    //         throw new BusinessRuleException("Zorunlu alanlar eksik.");
    //
    //     var user = new User
    //     {
    //         Email = importData["email"].ToLowerInvariant(),
    //         AnonymousUsername = GenerateAnonymousUsername(),
    //         HashedPassword = GenerateTemporaryPassword(),
    //         EncryptedFirstName = encryptionService.Encrypt(importData.GetValueOrDefault("firstName", "")),
    //         EncryptedLastName = encryptionService.Encrypt(importData.GetValueOrDefault("lastName", "")),
    //         EncryptedTcIdentityNumber = encryptionService.Encrypt(importData["tcno"]),
    //         // ... diğer alanlar
    //     };
    //
    //     // Import event'i
    //     user.AddDomainEvent(new UserImportedEvent(user.Id, "BulkImport", DateTime.UtcNow));
    //
    //     return user;
    // }

    /// <summary>
    /// Sosyal medya login'den kullanıcı oluşturur
    /// </summary>
    public static User CreateFromSocialLogin(string provider, string providerId, string email, string? name)
    {
        var user = new User
        {
            Email = email.ToLowerInvariant(), AnonymousUsername = GenerateAnonymousUsername()
            , HashedPassword = GenerateRandomPassword(), // Sosyal login için random
            EncryptedFirstName = "encrypted-" + (name?.Split(' ').FirstOrDefault() ?? "User")
            , EncryptedLastName = "encrypted-" + (name?.Split(' ').LastOrDefault() ?? "User"),
            // Diğer alanlar default/empty
            EncryptedTcIdentityNumber = "encrypted-pending"
            , EncryptedAddress = "encrypted-pending", EncryptedCity = "encrypted-pending"
            , EncryptedDistrict = "encrypted-pending", EncryptedPhoneNumber = "encrypted-pending"
            , Gender = Gender.PreferNotToSay, EncryptedBirthDate = "encrypted-pending", Profession = "Belirtilmemiş"
            , IsEmailVerified = true // Sosyal medya'dan geldiği için verified
        };

        user.EmailHash = GenerateHash(email.ToLowerInvariant());

        // Social login event
        user.AddDomainEvent(new UserRegisteredViaSocialEvent(user.Id, provider, providerId, user.CreatedAt));

        return user;
    }

    // Helper methods
    private static string GenerateAnonymousUsername()
    {
        var adjectives = new[] { "Gizli", "Anonim", "Özel", "Saklı", "Bilinmeyen" };
        var nouns = new[] { "Kullanıcı", "Yorumcu", "Üye", "Kişi", "Değerlendirici" };
        var random = new Random();
        return
            $"{adjectives[random.Next(adjectives.Length)]}{nouns[random.Next(nouns.Length)]}{random.Next(1000, 9999)}";
    }

    private static string GenerateTemporaryPassword() => $"Temp-{Guid.NewGuid():N}";
    private static string GenerateRandomPassword() => $"Social-{Guid.NewGuid():N}";

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
            null, // PreviousEmail
            "EmailToken", // VerificationMethod
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
    /// Blockchain kimlik bilgilerini ayarlar
    /// </summary>
    public void SetBlockchainIdentity(UserBlockchainIdentity blockchainIdentity)
    {
        if (blockchainIdentity == null)
            throw new ArgumentNullException(nameof(blockchainIdentity));
            
        if (IsBlockchainVerified)
            throw new BusinessRuleException("Blockchain kimliği zaten doğrulanmış.");
            
        BlockchainWalletAddress = blockchainIdentity.WalletAddress.Value;
        EncryptedBlockchainPrivateKey = blockchainIdentity.EncryptedPrivateKey;
        BlockchainPublicKey = blockchainIdentity.PublicKey;
        BlockchainIdentityContractAddress = blockchainIdentity.IdentityContractAddress;
        IsBlockchainVerified = true;
        BlockchainVerifiedAt = DateTime.UtcNow;
        SetModifiedDate();
        
        // Domain Event
        AddDomainEvent(new UserBlockchainIdentityCreatedEvent(
            Id,
            BlockchainWalletAddress,
            BlockchainPublicKey,
            DateTime.UtcNow
        ));
    }
    
    /// <summary>
    /// Blockchain kimlik sözleşme adresini günceller
    /// </summary>
    public void UpdateBlockchainContractAddress(string contractAddress)
    {
        if (string.IsNullOrWhiteSpace(contractAddress))
            throw new ArgumentNullException(nameof(contractAddress));
            
        if (!IsBlockchainVerified)
            throw new BusinessRuleException("Blockchain kimliği henüz oluşturulmamış.");
            
        BlockchainIdentityContractAddress = contractAddress;
        SetModifiedDate();
        
        // Domain Event
        AddDomainEvent(new UserBlockchainContractDeployedEvent(
            Id,
            contractAddress,
            DateTime.UtcNow
        ));
    }
    
    /// <summary>
    /// Blockchain kimliğini doğrular
    /// </summary>
    public void VerifyBlockchainIdentity()
    {
        if (!IsBlockchainVerified)
            throw new BusinessRuleException("Blockchain kimliği henüz oluşturulmamış.");
            
        BlockchainVerifiedAt = DateTime.UtcNow;
        SetModifiedDate();
        
        // Domain Event
        AddDomainEvent(new UserBlockchainIdentityVerifiedEvent(
            Id,
            BlockchainWalletAddress!,
            DateTime.UtcNow
        ));
    }
    
    /// <summary>
    /// Kullanıcının blockchain kimliği olup olmadığını kontrol eder
    /// </summary>
    public bool HasBlockchainIdentity()
    {
        return !string.IsNullOrWhiteSpace(BlockchainWalletAddress) 
            && !string.IsNullOrWhiteSpace(EncryptedBlockchainPrivateKey);
    }

    /// <summary>
    /// Telefon numarasını günceller
    /// </summary>
    public void UpdatePhoneNumber(string encryptedPhoneNumber)
    {
        if (string.IsNullOrEmpty(encryptedPhoneNumber))
            throw new ArgumentNullException(nameof(encryptedPhoneNumber));
            
        EncryptedPhoneNumber = encryptedPhoneNumber;
        IsPhoneVerified = false;
        PhoneVerificationCode = null;
        PhoneVerificationCodeExpiry = null;
        SetModifiedDate();
    }
    
    /// <summary>
    /// Kullanıcı profilini günceller
    /// </summary>
    public void UpdateProfile
    (
        string? profession = null
        , string? encryptedAddress = null
        , string? encryptedCity = null
        , string? encryptedDistrict = null
    )
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
                null, // OldValues - can be populated by application layer if needed
                null, // NewValues - can be populated by application layer if needed
                0.0m, // ProfileCompleteness - can be calculated by domain service
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
    /// Şifre belirle (test için)
    /// </summary>
    public void SetPassword(string hashedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
            throw new ArgumentNullException(nameof(hashedPassword));
            
        HashedPassword = hashedPassword;
        SetModifiedDate();
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
            0, // ReviewsCount - will be populated by application layer
            0, // BadgesCount - will be populated by application layer
            false, // DataExported - will be set by application layer
            null, // FeedbackProvided
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

        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            throw new BusinessRuleException("Kullanıcı adı sadece harf, rakam ve alt çizgi içerebilir.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new BusinessRuleException("Geçersiz email formatı.");
    }
}
