using System.Security.Cryptography;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Constants;
using RateTheWork.Domain.Enums.Admin;
using RateTheWork.Domain.Events.AdminUser;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Admin kullanıcı entity'si - Admin paneline erişen yöneticilerin hesap bilgilerini tutar.
/// </summary>
public class AdminUser : AuditableBaseEntity
{
    // Properties
    public string Username { get; private set; } = string.Empty;
    public string HashedPassword { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public AdminRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int FailedLoginAttempts { get; private set; } = 0;
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastFailedLoginAt { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public string? TwoFactorSecret { get; private set; }
    public bool IsTwoFactorEnabled { get; private set; } = false;
    public DateTime? PasswordChangedAt { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private AdminUser() : base()
    {
    }

    /// <summary>
    /// Yeni admin kullanıcı oluşturur (Factory method)
    /// </summary>
    public static AdminUser Create(
        string username,
        string email,
        string hashedPassword,
        AdminRole role,
        string createdByAdminId)
    {
        var roleString = role.ToString();
        ValidateUsername(username);
        ValidateEmail(email);
        ValidateRole(roleString);

        var adminUser = new AdminUser
        {
            Username = username,
            Email = email.ToLowerInvariant(),
            HashedPassword = hashedPassword ?? throw new ArgumentNullException(nameof(hashedPassword)),
            Role = role,
            IsActive = true,
            PasswordChangedAt = DateTime.UtcNow
        };

        adminUser.SetCreatedAudit(createdByAdminId);

        // Domain Event
        adminUser.AddDomainEvent(new AdminUserCreatedEvent(
            adminUser.Id,
            username,
            email,
            roleString,
            createdByAdminId,
            DateTime.UtcNow
        ));

        return adminUser;
    }

    /// <summary>
    /// Başarılı giriş
    /// </summary>
    public void RecordSuccessfulLogin(string ipAddress, string userAgent)
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LastFailedLoginAt = null;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new AdminLoginEvent(
            Id,
            ipAddress,
            userAgent,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Başarısız giriş denemesi
    /// </summary>
    public void RecordFailedLogin(string ipAddress)
    {
        FailedLoginAttempts++;
        LastFailedLoginAt = DateTime.UtcNow;

        // Hesap kilitleme kontrolü
        if (FailedLoginAttempts >= DomainConstants.Security.MaxFailedLoginAttempts)
        {
            LockAccount();
        }

        SetModifiedDate();

        // Domain Event - Username ile, Id kullanmıyoruz çünkü login başarısız
        AddDomainEvent(new AdminFailedLoginEvent(
            Username,
            ipAddress,
            FailedLoginAttempts,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Hesabı kilitle
    /// </summary>
    private void LockAccount()
    {
        LockedUntil = DateTime.UtcNow.AddMinutes(DomainConstants.Security.AccountLockMinutes);
        
        // Domain Event
        AddDomainEvent(new AdminAccountLockedEvent(
            Id,
            LockedUntil.Value,
            $"{DomainConstants.Security.MaxFailedLoginAttempts} başarısız giriş denemesi",
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Hesap kilitli mi kontrol et
    /// </summary>
    public bool IsLocked()
    {
        return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Rolü değiştir
    /// </summary>
    public void ChangeRole(string newRole, string changedBy)
    {
        ValidateRole(newRole);

        if (Role.ToString() == newRole)
            throw new BusinessRuleException("Yeni rol mevcut rol ile aynı.");

        var oldRole = Role.ToString();
        Role = Enum.Parse<AdminRole>(newRole);
        SetModifiedAudit(changedBy);

        // Domain Event
        AddDomainEvent(new AdminRoleChangedEvent(
            Id,
            oldRole,
            newRole,
            changedBy,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Şifre değiştir
    /// </summary>
    public void ChangePassword(string newHashedPassword)
    {
        if (string.IsNullOrEmpty(newHashedPassword))
            throw new ArgumentNullException(nameof(newHashedPassword));

        HashedPassword = newHashedPassword;
        PasswordChangedAt = DateTime.UtcNow;
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
        SetModifiedDate();
    }

    /// <summary>
    /// Şifre sıfırlama token'ı oluştur
    /// </summary>
    public string GeneratePasswordResetToken()
    {
        PasswordResetToken = Guid.NewGuid().ToString();
        PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);
        SetModifiedDate();
        return PasswordResetToken;
    }

    /// <summary>
    /// İki faktörlü doğrulamayı etkinleştir
    /// </summary>
    public string EnableTwoFactor()
    {
        if (IsTwoFactorEnabled)
            throw new BusinessRuleException("İki faktörlü doğrulama zaten aktif.");

        TwoFactorSecret = GenerateTwoFactorSecret();
        IsTwoFactorEnabled = true;
        SetModifiedDate();
        return TwoFactorSecret;
    }

    /// <summary>
    /// İki faktörlü doğrulamayı devre dışı bırak
    /// </summary>
    public void DisableTwoFactor()
    {
        if (!IsTwoFactorEnabled)
            throw new BusinessRuleException("İki faktörlü doğrulama zaten devre dışı.");

        TwoFactorSecret = null;
        IsTwoFactorEnabled = false;
        SetModifiedDate();
    }

    /// <summary>
    /// Hesabı aktif/pasif yap
    /// </summary>
    public void SetActiveStatus(bool isActive, string modifiedBy)
    {
        IsActive = isActive;
        SetModifiedAudit(modifiedBy);
    }

    // Private helper methods
    private static string GenerateTwoFactorSecret()
    {
        var key = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
        return Convert.ToBase64String(key);
    }

    // Validation methods
    private static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username));

        if (username.Length < 3 || username.Length > 50)
            throw new BusinessRuleException("Kullanıcı adı 3-50 karakter arasında olmalıdır.");

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

    private static void ValidateRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentNullException(nameof(role));

        var validRoles = new[] { "SuperAdmin", "Admin", "Moderator", "ContentManager" };
        if (!validRoles.Contains(role))
            throw new BusinessRuleException("Geçersiz rol.");
    }
}