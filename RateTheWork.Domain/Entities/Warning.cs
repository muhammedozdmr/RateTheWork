using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Uyarı entity'si - Bir kullanıcının aldığı uyarıları temsil eder.
/// </summary>
public class Warning : BaseEntity
{
    // Warning Types
    public enum WarningType
    {
        ContentViolation,      // İçerik ihlali
        SpamBehavior,         // Spam davranışı
        FalseInformation,     // Yanlış bilgi
        DisrespectfulBehavior, // Saygısız davranış
        Other                 // Diğer
    }

    // Warning Severity
    public enum WarningSeverity
    {
        Low,      // Düşük - Bilgilendirme amaçlı
        Medium,   // Orta - Dikkat edilmesi gereken
        High,     // Yüksek - Ciddi ihlal
        Critical  // Kritik - Ban'a yakın
    }

    // Properties
    public string UserId { get; private set; }
    public string AdminId { get; private set; }
    public string Reason { get; private set; }
    public string? DetailedExplanation { get; private set; }
    public WarningType Type { get; private set; }
    public WarningSeverity Severity { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public string? RelatedEntityType { get; private set; } // Review, Comment vb.
    public string? RelatedEntityId { get; private set; } // İlgili entity ID
    public bool IsAcknowledged { get; private set; } // Kullanıcı gördü mü?
    public DateTime? AcknowledgedAt { get; private set; }
    public bool IsActive { get; private set; } // Geçerli mi?
    public DateTime? ExpiresAt { get; private set; } // Ne zaman geçersiz olacak?
    public string? AppealNotes { get; private set; } // İtiraz notları
    public bool WasAppealed { get; private set; } // İtiraz edildi mi?
    public int Points { get; private set; } // Uyarı puanı (ağırlık)

    /// <summary>
    /// EF Core için private constructor
    /// </summary>
    private Warning(string userId, string adminId, string reason) : base()
    {
        UserId = userId;
        AdminId = adminId;
        Reason = reason;
    }

    /// <summary>
    /// Yeni uyarı oluşturur
    /// </summary>
    public static Warning Create(
        string userId,
        string adminId,
        string reason,
        WarningType type,
        WarningSeverity severity,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        string? detailedExplanation = null,
        int? expirationDays = null)
    {
        ValidateReason(reason);

        var warning = new Warning
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId)),
            AdminId = adminId ?? throw new ArgumentNullException(nameof(adminId)),
            Reason = reason,
            DetailedExplanation = detailedExplanation,
            Type = type,
            Severity = severity,
            IssuedAt = DateTime.UtcNow,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            IsAcknowledged = false,
            IsActive = true,
            WasAppealed = false
        };

        // Severity'ye göre puan hesapla
        warning.Points = warning.CalculatePoints();

        // Uyarı süresi (varsayılan: severity'ye göre)
        if (expirationDays.HasValue)
        {
            warning.ExpiresAt = warning.IssuedAt.AddDays(expirationDays.Value);
        }
        else
        {
            warning.SetDefaultExpiration();
        }

        // Domain Event
        warning.AddDomainEvent(new UserWarnedEvent(
            warning.Id,
            userId,
            adminId,
            reason,
            type,
            severity,
            warning.Points
        ));

        return warning;
    }

    /// <summary>
    /// Sistem otomatik uyarısı oluşturur
    /// </summary>
    public static Warning CreateSystemAutomatic(
        string userId,
        string reason,
        WarningType type,
        string? relatedEntityType = null,
        string? relatedEntityId = null)
    {
        return Create(
            userId,
            "SYSTEM",
            reason,
            type,
            WarningSeverity.Medium,
            relatedEntityType,
            relatedEntityId,
            "Bu uyarı sistem tarafından otomatik olarak oluşturulmuştur.",
            90 // 90 gün geçerli
        );
    }

    /// <summary>
    /// Kullanıcı uyarıyı gördü/onayladı
    /// </summary>
    public void Acknowledge(string userId)
    {
        if (UserId != userId)
            throw new BusinessRuleException("Sadece uyarı sahibi onaylayabilir.");

        if (IsAcknowledged)
            return;

        IsAcknowledged = true;
        AcknowledgedAt = DateTime.UtcNow;
        SetModifiedDate();

        AddDomainEvent(new WarningAcknowledgedEvent(Id, UserId));
    }

    /// <summary>
    /// Uyarıya itiraz et
    /// </summary>
    public void Appeal(string userId, string appealReason)
    {
        if (UserId != userId)
            throw new BusinessRuleException("Sadece uyarı sahibi itiraz edebilir.");

        if (!IsActive)
            throw new BusinessRuleException("Aktif olmayan uyarıya itiraz edilemez.");

        if (WasAppealed)
            throw new BusinessRuleException("Bu uyarıya zaten itiraz edilmiş.");

        ValidateAppealReason(appealReason);

        WasAppealed = true;
        AppealNotes = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Kullanıcı itirazı: {appealReason}";
        SetModifiedDate();

        AddDomainEvent(new WarningAppealedEvent(Id, UserId, appealReason));
    }

    /// <summary>
    /// Admin itirazı değerlendirir
    /// </summary>
    public void ReviewAppeal(string adminId, bool accepted, string reviewNotes)
    {
        if (!WasAppealed)
            throw new BusinessRuleException("İtiraz edilmemiş uyarı değerlendirilemez.");

        if (!IsActive)
            throw new BusinessRuleException("Aktif olmayan uyarı değerlendirilemez.");

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        AppealNotes += $"\n[{timestamp}] Admin değerlendirmesi ({adminId}): {reviewNotes}";

        if (accepted)
        {
            Revoke(adminId, "İtiraz kabul edildi: " + reviewNotes);
        }
        else
        {
            AppealNotes += "\n[SONUÇ: İtiraz reddedildi]";
            SetModifiedDate();
        }

        AddDomainEvent(new WarningAppealReviewedEvent(
            Id,
            UserId,
            adminId,
            accepted,
            reviewNotes
        ));
    }

    /// <summary>
    /// Uyarıyı iptal et
    /// </summary>
    public void Revoke(string revokedBy, string reason)
    {
        if (!IsActive)
            return;

        ValidateRevokeReason(reason);

        IsActive = false;
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        DetailedExplanation = $"{DetailedExplanation}\n[{timestamp}] İptal edildi ({revokedBy}): {reason}";
        SetModifiedDate();

        AddDomainEvent(new WarningRevokedEvent(
            Id,
            UserId,
            revokedBy,
            reason
        ));
    }

    /// <summary>
    /// Uyarı süresini uzat
    /// </summary>
    public void ExtendExpiration(int additionalDays, string extendedBy, string reason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Aktif olmayan uyarının süresi uzatılamaz.");

        if (additionalDays <= 0)
            throw new BusinessRuleException("Ek süre 0'dan büyük olmalıdır.");

        if (!ExpiresAt.HasValue)
        {
            ExpiresAt = DateTime.UtcNow.AddDays(additionalDays);
        }
        else
        {
            ExpiresAt = ExpiresAt.Value.AddDays(additionalDays);
        }

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        DetailedExplanation = $"{DetailedExplanation}\n[{timestamp}] Süre uzatıldı ({extendedBy}): {reason}";
        SetModifiedDate();
    }

    /// <summary>
    /// Uyarının geçerli olup olmadığını kontrol et
    /// </summary>
    public bool CheckIfActive()
    {
        if (!IsActive)
            return false;

        // Süresi dolmuşsa
        if (ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt.Value)
        {
            IsActive = false;
            SetModifiedDate();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Uyarı puanını hesapla
    /// </summary>
    private int CalculatePoints()
    {
        return Severity switch
        {
            WarningSeverity.Low => 1,
            WarningSeverity.Medium => 3,
            WarningSeverity.High => 5,
            WarningSeverity.Critical => 10,
            _ => 1
        };
    }

    /// <summary>
    /// Varsayılan süre belirleme
    /// </summary>
    private void SetDefaultExpiration()
    {
        var days = Severity switch
        {
            WarningSeverity.Low => 30,      // 30 gün
            WarningSeverity.Medium => 90,    // 90 gün
            WarningSeverity.High => 180,     // 180 gün
            WarningSeverity.Critical => 365, // 1 yıl
            _ => 90
        };

        ExpiresAt = IssuedAt.AddDays(days);
    }

    /// <summary>
    /// Uyarı özetini döndür
    /// </summary>
    public string GetSummary()
    {
        var summary = $"[{Severity}] {Type}: {Reason}";
        
        if (RelatedEntityType != null && RelatedEntityId != null)
        {
            summary += $" (İlgili: {RelatedEntityType} #{RelatedEntityId})";
        }

        if (!IsActive)
        {
            summary += " [İPTAL EDİLDİ]";
        }
        else if (ExpiresAt.HasValue)
        {
            summary += $" (Geçerlilik: {ExpiresAt.Value:dd.MM.yyyy})";
        }

        return summary;
    }

    /// <summary>
    /// Kalan süreyi hesapla
    /// </summary>
    public TimeSpan? GetRemainingTime()
    {
        if (!IsActive || !ExpiresAt.HasValue)
            return null;

        var remaining = ExpiresAt.Value - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    // Validation methods
    private static void ValidateReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        if (reason.Length < 10)
            throw new BusinessRuleException("Uyarı nedeni en az 10 karakter olmalıdır.");

        if (reason.Length > 500)
            throw new BusinessRuleException("Uyarı nedeni 500 karakterden fazla olamaz.");
    }

    private static void ValidateAppealReason(string appealReason)
    {
        if (string.IsNullOrWhiteSpace(appealReason))
            throw new ArgumentNullException(nameof(appealReason));

        if (appealReason.Length < 20)
            throw new BusinessRuleException("İtiraz nedeni en az 20 karakter olmalıdır.");

        if (appealReason.Length > 1000)
            throw new BusinessRuleException("İtiraz nedeni 1000 karakterden fazla olamaz.");
    }

    private static void ValidateRevokeReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        if (reason.Length < 10)
            throw new BusinessRuleException("İptal nedeni en az 10 karakter olmalıdır.");
    }
}