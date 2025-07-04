using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Ban entity'si - Bir kullanıcının banlanma kaydını temsil eder.
/// </summary>
public class Ban : BaseEntity
{
    // Ban Types
    public enum BanType
    {
        Temporary,      // Geçici ban (süre belirtilir)
        Permanent,      // Kalıcı ban
        SystemAutomatic // Sistem otomatik banı
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

    // Properties
    public string UserId { get; private set; }
    public string AdminId { get; private set; }
    public string Reason { get; private set; }
    public string? DetailedReason { get; private set; }
    public DateTime BannedAt { get; private set; }
    public DateTime? UnbanDate { get; private set; } // Süresizse null
    public BanType Type { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LiftedAt { get; private set; } // Ban kaldırıldıysa
    public string? LiftedBy { get; private set; } // Kim kaldırdı
    public string? LiftReason { get; private set; } // Neden kaldırıldı
    public bool IsAppealable { get; private set; } // İtiraz edilebilir mi?
    public DateTime? AppealDeadline { get; private set; } // İtiraz son tarihi
    public string? AppealNotes { get; private set; } // İtiraz notları

    /// <summary>
    /// EF Core için private constructor
    /// </summary>
    private Ban(string userId, string adminId, string reason) : base()
    {
        UserId = userId;
        AdminId = adminId;
        Reason = reason;
    }

    /// <summary>
    /// Geçici ban oluşturur
    /// </summary>
    public static Ban CreateTemporary(
        string userId,
        string adminId,
        string reason,
        int durationDays,
        string? detailedReason = null,
        bool isAppealable = true)
    {
        if (durationDays <= 0)
            throw new BusinessRuleException("Ban süresi 0'dan büyük olmalıdır.");

        if (durationDays > 365)
            throw new BusinessRuleException("Geçici ban süresi 1 yıldan fazla olamaz. Kalıcı ban kullanın.");

        var ban = CreateBase(userId, adminId, reason, BanType.Temporary, detailedReason, isAppealable);
        ban.UnbanDate = ban.BannedAt.AddDays(durationDays);

        if (isAppealable)
        {
            // İtiraz süresi ban süresinin %20'si veya max 7 gün
            var appealDays = Math.Min(durationDays * 0.2, 7);
            ban.AppealDeadline = ban.BannedAt.AddDays(appealDays);
        }

        return ban;
    }

    /// <summary>
    /// Kalıcı ban oluşturur
    /// </summary>
    public static Ban CreatePermanent(
        string userId,
        string adminId,
        string reason,
        string? detailedReason = null,
        bool isAppealable = false)
    {
        var ban = CreateBase(userId, adminId, reason, BanType.Permanent, detailedReason, isAppealable);
        
        if (isAppealable)
        {
            ban.AppealDeadline = ban.BannedAt.AddDays(30); // 30 gün itiraz süresi
        }

        return ban;
    }

    /// <summary>
    /// Sistem otomatik banı oluşturur
    /// </summary>
    public static Ban CreateSystemAutomatic(
        string userId,
        int warningCount,
        int durationDays = 7)
    {
        var ban = CreateBase(
            userId,
            "SYSTEM",
            BanReasons.AutomaticWarningLimit,
            BanType.SystemAutomatic,
            $"Kullanıcı {warningCount} uyarı aldı ve otomatik olarak banlandı.",
            true // Her zaman itiraz edilebilir
        );

        ban.UnbanDate = ban.BannedAt.AddDays(durationDays);
        ban.AppealDeadline = ban.BannedAt.AddDays(3); // 3 gün itiraz süresi

        return ban;
    }

    /// <summary>
    /// Ban'ı kaldırır
    /// </summary>
    public void Lift(string liftedByAdminId, string reason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Bu ban zaten kaldırılmış.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleException("Ban kaldırma nedeni belirtilmelidir.");

        IsActive = false;
        LiftedAt = DateTime.UtcNow;
        LiftedBy = liftedByAdminId;
        LiftReason = reason;
        SetModifiedDate();

        AddDomainEvent(new BanLiftedEvent(
            Id,
            UserId,
            liftedByAdminId,
            reason,
            Type
        ));
    }

    /// <summary>
    /// Ban süresini uzatır (sadece geçici banlar için)
    /// </summary>
    public void ExtendDuration(int additionalDays, string extendedBy, string reason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Aktif olmayan ban uzatılamaz.");

        if (Type != BanType.Temporary)
            throw new BusinessRuleException("Sadece geçici banlar uzatılabilir.");

        if (!UnbanDate.HasValue)
            throw new BusinessRuleException("Ban süresi bilgisi eksik.");

        if (additionalDays <= 0)
            throw new BusinessRuleException("Ek süre 0'dan büyük olmalıdır.");

        var newUnbanDate = UnbanDate.Value.AddDays(additionalDays);
        var totalDuration = (newUnbanDate - BannedAt).TotalDays;

        if (totalDuration > 365)
            throw new BusinessRuleException("Toplam ban süresi 1 yılı geçemez.");

        UnbanDate = newUnbanDate;
        DetailedReason = $"{DetailedReason}\n[{DateTime.UtcNow:yyyy-MM-dd}] Süre uzatıldı: {reason} (Admin: {extendedBy})";
        SetModifiedDate();

        AddDomainEvent(new BanExtendedEvent(
            Id,
            UserId,
            extendedBy,
            additionalDays,
            reason
        ));
    }

    /// <summary>
    /// İtiraz notu ekler
    /// </summary>
    public void AddAppealNote(string note, string addedBy)
    {
        if (!IsAppealable)
            throw new BusinessRuleException("Bu ban'a itiraz edilemez.");

        if (AppealDeadline.HasValue && DateTime.UtcNow > AppealDeadline.Value)
            throw new BusinessRuleException("İtiraz süresi dolmuştur.");

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var newNote = $"[{timestamp}] {addedBy}: {note}";

        AppealNotes = string.IsNullOrWhiteSpace(AppealNotes)
            ? newNote
            : $"{AppealNotes}\n{newNote}";

        SetModifiedDate();
    }

    /// <summary>
    /// Ban'ı kalıcı yapar
    /// </summary>
    public void MakePermanent(string adminId, string reason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Aktif olmayan ban kalıcı yapılamaz.");

        if (Type == BanType.Permanent)
            return; // Zaten kalıcı

        Type = BanType.Permanent;
        UnbanDate = null;
        IsAppealable = false;
        AppealDeadline = null;
        DetailedReason = $"{DetailedReason}\n[{DateTime.UtcNow:yyyy-MM-dd}] Kalıcı yapıldı: {reason} (Admin: {adminId})";
        SetModifiedDate();

        AddDomainEvent(new BanMadePermanentEvent(Id, UserId, adminId, reason));
    }

    /// <summary>
    /// Ban'ın aktif olup olmadığını kontrol eder
    /// </summary>
    public bool CheckIfActive()
    {
        if (!IsActive)
            return false;

        // Geçici ban ve süresi dolmuşsa
        if (Type == BanType.Temporary && UnbanDate.HasValue && DateTime.UtcNow >= UnbanDate.Value)
        {
            IsActive = false;
            SetModifiedDate();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Kalan ban süresini hesaplar
    /// </summary>
    public TimeSpan? GetRemainingDuration()
    {
        if (!IsActive || !UnbanDate.HasValue)
            return null;

        var remaining = UnbanDate.Value - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Ban açıklamasını döndürür
    /// </summary>
    public string GetDescription()
    {
        var desc = $"{Reason}";
        
        if (Type == BanType.Temporary && UnbanDate.HasValue)
        {
            desc += $" (Bitiş: {UnbanDate.Value:dd.MM.yyyy})";
        }
        else if (Type == BanType.Permanent)
        {
            desc += " (Kalıcı)";
        }

        if (!IsActive)
        {
            desc += " [KALDIRILDI]";
        }

        return desc;
    }

    // Private helper method
    private static Ban CreateBase(
        string userId,
        string adminId,
        string reason,
        BanType type,
        string? detailedReason,
        bool isAppealable)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (string.IsNullOrWhiteSpace(adminId))
            throw new ArgumentNullException(nameof(adminId));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        var ban = new Ban
        {
            UserId = userId,
            AdminId = adminId,
            Reason = reason,
            DetailedReason = detailedReason,
            BannedAt = DateTime.UtcNow,
            Type = type,
            IsActive = true,
            IsAppealable = isAppealable
        };

        // Domain Event
        ban.AddDomainEvent(new UserBannedEvent(
            ban.Id,
            userId,
            adminId,
            reason,
            type,
            ban.UnbanDate
        ));

        return ban;
    }
}