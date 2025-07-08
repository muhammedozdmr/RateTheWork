using System.Security.Cryptography;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Admin kullanıcı entity'si - Admin paneline erişen yöneticilerin hesap bilgilerini tutar.
/// </summary>
public class AdminUser : AuditableBaseEntity
{
    // Constants
    private const int MinPasswordLength = 8;
    private const int MaxFailedLoginAttempts = 5;
    private const int AccountLockMinutes = 30;

    // Properties
    public string? Username { get; private set; } = string.Empty;
    public string? HashedPassword { get; private set; } = string.Empty;
    public string? Email { get; private set; } = string.Empty;
    public string? Role { get; private set; } = string.Empty; // SuperAdmin, Moderator, ContentManager
    public bool IsActive { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastFailedLoginAt { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public string? TwoFactorSecret { get; private set; } = string.Empty;
    public bool IsTwoFactorEnabled { get; private set; }
    public DateTime? PasswordChangedAt { get; private set; }
    public string? PasswordResetToken { get; private set; } = string.Empty;
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private AdminUser() : base()
    {
    }

    /// <summary>
    /// Factory method için private constructor
    /// </summary>
    private AdminUser(string username, string hashedPassword, string email, string role) : base()
    {
        Username = username;
        HashedPassword = hashedPassword;
        Email = email;
        Role = role;
    }

    /// <summary>
    /// Yeni admin kullanıcı oluşturur
    /// </summary>
    public static AdminUser Create
    (
        string username
        , string? email
        , string hashedPassword
        , string? role
        , string createdByAdminId
    )
    {
        ValidateUsername(username);
        ValidateEmail(email);
        ValidateRole(role);

        var adminUser = new AdminUser
        {
            Username = username, Email = email
            , HashedPassword = hashedPassword ?? throw new ArgumentNullException(nameof(hashedPassword)), Role = role
            , IsActive = true, FailedLoginAttempts = 0, IsTwoFactorEnabled = false, PasswordChangedAt = DateTime.UtcNow
        };

        adminUser.SetCreationAuditInfo(createdByAdminId);

        // Domain Event
        adminUser.AddDomainEvent(new AdminUserCreatedEvent(
            adminUser.Id,
            username,
            email,
            role,
            createdByAdminId
        ));

        return adminUser;
    }

    /// <summary>
    /// Başarılı giriş işlemi
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LastLoginAt = DateTime.UtcNow;
        LastFailedLoginAt = null;
        LockedUntil = null;
        SetModifiedDate();

        AddDomainEvent(new AdminLoginSuccessEvent(Id, Username, LastLoginAt.Value));
    }

    /// <summary>
    /// Başarısız giriş işlemi
    /// </summary>
    public void RecordFailedLogin(string ipAddress)
    {
        FailedLoginAttempts++;
        LastFailedLoginAt = DateTime.UtcNow;

        // Hesap kilitleme kontrolü
        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
        {
            LockAccount();
        }

        SetModifiedDate();

        AddDomainEvent(new AdminLoginFailedEvent(
            Id,
            Username,
            ipAddress,
            FailedLoginAttempts,
            IsAccountLocked()
        ));
    }

    /// <summary>
    /// Hesabı kilitler
    /// </summary>
    private void LockAccount()
    {
        LockedUntil = DateTime.UtcNow.AddMinutes(AccountLockMinutes);
        AddDomainEvent(new AdminAccountLockedEvent(
            Id,
            Username,
            LockedUntil.Value,
            $"{MaxFailedLoginAttempts} başarısız giriş denemesi"
        ));
    }

    /// <summary>
    /// Hesap kilitli mi kontrol eder
    /// </summary>
    public bool IsAccountLocked()
    {
        return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Hesap kilidini açar
    /// </summary>
    public void UnlockAccount(string unlockedByAdminId)
    {
        if (!IsAccountLocked())
            return;

        LockedUntil = null;
        FailedLoginAttempts = 0;
        SetModifiedDate();

        AddDomainEvent(new AdminAccountUnlockedEvent(Id, Username, unlockedByAdminId));
    }

    /// <summary>
    /// Şifre değiştirir
    /// </summary>
    public void ChangePassword(string? newHashedPassword)
    {
        if (string.IsNullOrWhiteSpace(newHashedPassword))
            throw new ArgumentNullException(nameof(newHashedPassword));

        if (HashedPassword == newHashedPassword)
            throw new BusinessRuleException("Yeni şifre eski şifre ile aynı olamaz.");

        HashedPassword = newHashedPassword;
        PasswordChangedAt = DateTime.UtcNow;
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
        SetModifiedDate();

        AddDomainEvent(new AdminPasswordChangedEvent(Id, Username));
    }

    /// <summary>
    /// Şifre sıfırlama token'ı oluşturur
    /// </summary>
    public string GeneratePasswordResetToken()
    {
        PasswordResetToken = GenerateSecureToken();
        PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(2); // 2 saat geçerli
        SetModifiedDate();

        return PasswordResetToken;
    }

    /// <summary>
    /// Şifre sıfırlama token'ını doğrular
    /// </summary>
    public bool ValidatePasswordResetToken(string token)
    {
        if (string.IsNullOrWhiteSpace(PasswordResetToken))
            return false;

        if (PasswordResetTokenExpiry.HasValue && PasswordResetTokenExpiry.Value < DateTime.UtcNow)
            return false;

        return PasswordResetToken == token;
    }

    /// <summary>
    /// 2FA etkinleştirir
    /// </summary>
    public string EnableTwoFactorAuthentication()
    {
        if (IsTwoFactorEnabled)
            throw new BusinessRuleException("İki faktörlü doğrulama zaten etkin.");

        TwoFactorSecret = GenerateSecureToken();
        IsTwoFactorEnabled = true;
        SetModifiedDate();

        AddDomainEvent(new AdminTwoFactorEnabledEvent(Id, Username));

        return TwoFactorSecret;
    }

    /// <summary>
    /// 2FA devre dışı bırakır
    /// </summary>
    public void DisableTwoFactorAuthentication(string disabledByAdminId)
    {
        if (!IsTwoFactorEnabled)
            return;

        TwoFactorSecret = null;
        IsTwoFactorEnabled = false;
        SetModifiedDate();

        AddDomainEvent(new AdminTwoFactorDisabledEvent(Id, Username, disabledByAdminId));
    }

    /// <summary>
    /// Admin rolünü değiştirir
    /// </summary>
    public void ChangeRole(string? newRole, string changedByAdminId)
    {
        ValidateRole(newRole);

        if (Role == newRole)
            return;

        var oldRole = Role;
        Role = newRole;
        SetModifiedDate();

        AddDomainEvent(new AdminRoleChangedEvent(Id, Username, oldRole, newRole, changedByAdminId));
    }

    /// <summary>
    /// Admin hesabını deaktive eder
    /// </summary>
    public void Deactivate(string deactivatedByAdminId, string reason)
    {
        if (!IsActive)
            return;

        IsActive = false;
        SetModifiedDate();

        AddDomainEvent(new AdminDeactivatedEvent(Id, Username, deactivatedByAdminId, reason));
    }

    /// <summary>
    /// Admin hesabını aktive eder
    /// </summary>
    public void Activate(string activatedByAdminId)
    {
        if (IsActive)
            return;

        IsActive = true;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        SetModifiedDate();

        AddDomainEvent(new AdminActivatedEvent(Id, Username, activatedByAdminId));
    }

    /// <summary>
    /// Email günceller
    /// </summary>
    public void UpdateEmail(string? newEmail, string updatedByAdminId)
    {
        ValidateEmail(newEmail);

        if (Email == newEmail)
            return;

        var oldEmail = Email;
        Email = newEmail;
        SetModifiedDate();

        AddDomainEvent(new AdminEmailChangedEvent(Id, Username, oldEmail, newEmail, updatedByAdminId));
    }

    // Validation methods
    private static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username));

        if (username.Length < 3)
            throw new BusinessRuleException("Kullanıcı adı en az 3 karakter olmalıdır.");

        if (username.Length > 50)
            throw new BusinessRuleException("Kullanıcı adı en fazla 50 karakter olabilir.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            throw new BusinessRuleException("Kullanıcı adı sadece harf, rakam ve alt çizgi içerebilir.");
    }

    private static void ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new BusinessRuleException("Geçersiz email formatı.");
    }

    private static void ValidateRole(string? role)
    {
        var validRoles = new[] { "SuperAdmin", "Moderator", "ContentManager" };
        if (!validRoles.Contains(role))
            throw new BusinessRuleException($"Geçersiz rol: {role}. Geçerli roller: {string.Join(", ", validRoles)}");
    }

    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
