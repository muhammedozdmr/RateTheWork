using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.User;
using RateTheWork.Domain.Events.Ban;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Kullanıcı yasaklama entity'si
/// </summary>
public class Ban : BaseEntity
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Ban() : base()
    {
    }

    // Properties
    public string UserId { get; private set; } = string.Empty;
    public string AdminUserId { get; private set; } = string.Empty;
    public BanType Type { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime BannedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsPermanent { get; private set; }
    public string? AppealNotes { get; private set; }
    public DateTime? AppealedAt { get; private set; }
    public string? LiftedBy { get; private set; }
    public DateTime? LiftedAt { get; private set; }
    public string? LiftReason { get; private set; }
    
    // Eski kodlarla uyumluluk için alias'lar
    public DateTime? UnbanDate => ExpiresAt;
    public string TargetType => "User"; // Ban her zaman kullanıcıya yapılır
    public string TargetId => UserId;

    /// <summary>
    /// Yeni ban oluşturur (Factory method)
    /// </summary>
    public static Ban Create(
        string userId,
        string adminUserId,
        BanType type,
        string reason,
        int? banDays = null)
    {
        // Validasyonlar
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));
            
        if (string.IsNullOrWhiteSpace(adminUserId))
            throw new ArgumentNullException(nameof(adminUserId));
            
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));
            
        if (reason.Length < 10)
            throw new BusinessRuleException("Ban açıklaması en az 10 karakter olmalıdır.");
            
        if (reason.Length > 1000)
            throw new BusinessRuleException("Ban açıklaması 1000 karakterden uzun olamaz.");

        var ban = new Ban
        {
            UserId = userId,
            AdminUserId = adminUserId,
            Type = type,
            Reason = reason,
            BannedAt = DateTime.UtcNow,
            IsPermanent = !banDays.HasValue,
            ExpiresAt = banDays.HasValue ? DateTime.UtcNow.AddDays(banDays.Value) : null
        };

        // Domain event
        ban.AddDomainEvent(new UserBannedEvent(
            ban.Id,
            userId,
            adminUserId,
            reason,
            type.ToString(),
            ban.ExpiresAt,
            true, // isAppealable
            ban.BannedAt
        ));

        return ban;
    }

    /// <summary>
    /// Ban'e itiraz et
    /// </summary>
    public void Appeal(string appealNotes)
    {
        if (!IsActive)
            throw new BusinessRuleException("Aktif olmayan bir ban'e itiraz edilemez.");
            
        if (AppealedAt.HasValue)
            throw new BusinessRuleException("Bu ban'e zaten itiraz edilmiş.");

        if (string.IsNullOrWhiteSpace(appealNotes) || appealNotes.Length < 20)
            throw new BusinessRuleException("İtiraz metni en az 20 karakter olmalıdır.");

        AppealNotes = appealNotes;
        AppealedAt = DateTime.UtcNow;
        SetModifiedDate();
    }

    /// <summary>
    /// Ban'i kaldır
    /// </summary>
    public void Lift(string adminUserId, string reason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Bu ban zaten kaldırılmış.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleException("Ban kaldırma nedeni belirtilmelidir.");

        IsActive = false;
        LiftedBy = adminUserId;
        LiftedAt = DateTime.UtcNow;
        LiftReason = reason;
        SetModifiedDate();

        // Domain event
        AddDomainEvent(new BanLiftedEvent(
            Id,
            UserId,
            adminUserId,
            reason,
            LiftedAt.Value
        ));
    }

    /// <summary>
    /// Ban'in süresi dolmuş mu?
    /// </summary>
    public bool IsExpired()
    {
        return !IsPermanent && ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }
}