using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.User;
using RateTheWork.Domain.Events.Ban;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Ban entity'si - Bir kullanıcının banlanma kaydını temsil eder.
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
    public string AdminId { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string? DetailedReason { get; private set; }
    public DateTime BannedAt { get; private set; }
    public DateTime? UnbanDate { get; private set; }
    public BanType Type { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LiftedAt { get; private set; }
    public string? LiftedBy { get; private set; }
    public string? LiftReason { get; private set; }
    public bool IsAppealable { get; private set; } = true;
    public DateTime? AppealDeadline { get; private set; }
    public string? AppealNotes { get; private set; }
    public string? TargetType { get; private set; }
    public string? TargetId { get; private set; }

    /// <summary>
    /// Geçici ban oluşturur (Factory method)
    /// </summary>
    public static Ban CreateTemporary
    (
        string userId
        , string adminId
        , string reason
        , int durationDays
        , string? detailedReason = null
        , bool isAppealable = true
    )
    {
        if (durationDays <= 0)
            throw new BusinessRuleException("Ban süresi 0'dan büyük olmalıdır.");

        if (durationDays > 365)
            throw new BusinessRuleException("Geçici ban süresi 1 yıldan fazla olamaz. Kalıcı ban kullanın.");

        var ban = new Ban
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId))
            , AdminId = adminId ?? throw new ArgumentNullException(nameof(adminId))
            , Reason = ValidateAndReturnReason(reason), DetailedReason = detailedReason, BannedAt = DateTime.UtcNow
            , UnbanDate = DateTime.UtcNow.AddDays(durationDays), Type = BanType.Temporary, IsActive = true
            , IsAppealable = isAppealable, AppealDeadline = isAppealable ? DateTime.UtcNow.AddDays(7) : null
        };

        // Domain Event
        ban.AddDomainEvent(new UserBannedEvent(
            ban.Id,
            userId,
            adminId,
            reason,
            BanType.Temporary.ToString(),
            ban.UnbanDate,
            isAppealable,
            ban.BannedAt,
            DateTime.UtcNow
        ));

        return ban;
    }

    /// <summary>
    /// Kalıcı ban oluşturur
    /// </summary>
    public static Ban CreatePermanent
    (
        string userId
        , string adminId
        , string reason
        , string? detailedReason = null
        , bool isAppealable = false
    )
    {
        var ban = new Ban
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId))
            , AdminId = adminId ?? throw new ArgumentNullException(nameof(adminId))
            , Reason = ValidateAndReturnReason(reason), DetailedReason = detailedReason, BannedAt = DateTime.UtcNow
            , UnbanDate = null, Type = BanType.Permanent, IsActive = true, IsAppealable = isAppealable
            , AppealDeadline = isAppealable ? DateTime.UtcNow.AddDays(30) : null
        };

        // Domain Event
        ban.AddDomainEvent(new UserBannedEvent(
            ban.Id,
            userId,
            adminId,
            reason,
            BanType.Permanent.ToString(),
            null,
            isAppealable,
            ban.BannedAt,
            DateTime.UtcNow
        ));

        return ban;
    }

    /// <summary>
    /// Otomatik sistem banı oluşturur
    /// </summary>
    public static Ban CreateAutomatic
    (
        string userId
        , string triggerReason
        , int warningCount
    )
    {
        var ban = new Ban
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId)), AdminId = "SYSTEM"
            , Reason = BanReasons.AutomaticWarningLimit, DetailedReason = $"{triggerReason} - {warningCount} uyarı"
            , BannedAt = DateTime.UtcNow, UnbanDate = DateTime.UtcNow.AddDays(30), // 30 gün otomatik ban
            Type = BanType.SystemAutomatic
            , IsActive = true, IsAppealable = true, AppealDeadline = DateTime.UtcNow.AddDays(7)
        };

        // Domain Event
        ban.AddDomainEvent(new AutoBanCreatedEvent(
            ban.Id,
            userId,
            triggerReason,
            warningCount,
            ban.BannedAt,
            DateTime.UtcNow
        ));

        return ban;
    }

    /// <summary>
    /// Ban'ı kaldır
    /// </summary>
    public void Lift(string liftedBy, string liftReason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Ban zaten kaldırılmış.");

        if (string.IsNullOrWhiteSpace(liftReason))
            throw new ArgumentNullException(nameof(liftReason));

        IsActive = false;
        LiftedAt = DateTime.UtcNow;
        LiftedBy = liftedBy;
        LiftReason = liftReason;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new UserUnbannedEvent(
            Id,
            UserId,
            liftedBy,
            liftReason,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Ban'a itiraz et
    /// </summary>
    public void Appeal(string appealNotes)
    {
        if (!IsAppealable)
            throw new BusinessRuleException("Bu ban'a itiraz edilemez.");

        if (!IsActive)
            throw new BusinessRuleException("Aktif olmayan ban'a itiraz edilemez.");

        if (AppealDeadline.HasValue && DateTime.UtcNow > AppealDeadline.Value)
            throw new BusinessRuleException("İtiraz süresi dolmuş.");

        if (!string.IsNullOrWhiteSpace(AppealNotes))
            throw new BusinessRuleException("Bu ban'a zaten itiraz edilmiş.");

        AppealNotes = appealNotes;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new BanAppealedEvent(
            Id,
            UserId,
            appealNotes,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Ban hala aktif mi kontrol et
    /// </summary>
    public bool IsEffective()
    {
        if (!IsActive)
            return false;

        if (Type == BanType.Temporary && UnbanDate.HasValue && UnbanDate.Value <= DateTime.UtcNow)
            return false;

        return true;
    }

    // Private validation method
    private static string ValidateAndReturnReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        if (reason.Length < 5)
            throw new BusinessRuleException("Ban nedeni en az 5 karakter olmalıdır.");

        return reason;
    }

    // Ban Reasons
    public static class BanReasons
    {
        public const string MultipleViolations = "Çoklu kural ihlali";
        public const string SpamContent = "Spam içerik";
        public const string InappropriateContent = "Uygunsuz içerik";
        public const string FakeReviews = "Sahte yorumlar";
        public const string Harassment = "Taciz/Zorbalık";
        public const string IdentityFraud = "Kimlik sahtekarlığı";
        public const string AutomaticWarningLimit = "Otomatik: Uyarı limiti aşıldı";
        public const string Other = "Diğer";
    }
}
