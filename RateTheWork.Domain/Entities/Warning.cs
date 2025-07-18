using System.Text.Json;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.User;
using RateTheWork.Domain.Events.Warning;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Uyarı entity'si - Bir kullanıcının aldığı uyarıları temsil eder.
/// </summary>
public class Warning : BaseEntity
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Warning() : base()
    {
    }

    // Properties
    public string UserId { get; private set; } = string.Empty;
    public string AdminId { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string? DetailedExplanation { get; private set; }
    public WarningType Type { get; private set; }
    public WarningSeverity Severity { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public string? RelatedEntityId { get; private set; }
    public bool IsAcknowledged { get; private set; } = false;
    public DateTime? AcknowledgedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? ExpiresAt { get; private set; }
    public string? AppealNotes { get; private set; }
    public bool WasAppealed { get; private set; } = false;
    public int Points { get; private set; }

    /// <summary>
    /// Yeni uyarı oluşturur (Factory method)
    /// </summary>
    public static Warning Create
    (
        string userId
        , string adminId
        , string reason
        , WarningType type
        , WarningSeverity severity
        , string? relatedEntityType = null
        , string? relatedEntityId = null
        , string? detailedExplanation = null
        , int? expirationDays = null
    )
    {
        ValidateReason(reason);

        var warning = new Warning
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId))
            , AdminId = adminId ?? throw new ArgumentNullException(nameof(adminId)), Reason = reason, Type = type
            , Severity = severity, DetailedExplanation = detailedExplanation, RelatedEntityType = relatedEntityType
            , RelatedEntityId = relatedEntityId, IssuedAt = DateTime.UtcNow, IsActive = true
            , Points = CalculatePoints(severity)
            , ExpiresAt = expirationDays.HasValue ? DateTime.UtcNow.AddDays(expirationDays.Value) : null
        };

        // Domain Event
        warning.AddDomainEvent(new UserWarnedEvent(
            warning.Id,
            userId,
            adminId,
            reason,
            type.ToString(),
            severity.ToString(),
            warning.Points,
            0, // TotalWarnings will be calculated by handler
            warning.IssuedAt
        ));

        return warning;
    }

    /// <summary>
    /// İçerik ihlali uyarısı oluşturur
    /// </summary>
    public static Warning CreateForContentViolation
    (
        string userId
        , string adminId
        , string violatedContent
        , string contentId
        , WarningSeverity severity
        , string? detailedExplanation = null
    )
    {
        var reason = $"İçerik ihlali: {violatedContent}";

        return Create(
            userId,
            adminId,
            reason,
            WarningType.ContentViolation,
            severity,
            "Content",
            contentId,
            detailedExplanation,
            severity == WarningSeverity.Low ? 30 : null // Düşük seviyeli uyarılar 30 gün sonra sona erer
        );
    }

    /// <summary>
    /// Spam davranışı uyarısı oluşturur
    /// </summary>
    public static Warning CreateForSpamBehavior
    (
        string userId
        , string adminId
        , string spamType
        , int spamCount
        , string? relatedEntityId = null
    )
    {
        var reason = $"Spam davranışı tespit edildi: {spamType} ({spamCount} kez)";
        var severity = spamCount switch
        {
            < 3 => WarningSeverity.Low, < 5 => WarningSeverity.Medium, < 10 => WarningSeverity.High
            , _ => WarningSeverity.Critical
        };

        return Create(
            userId,
            adminId,
            reason,
            WarningType.SpamBehavior,
            severity,
            "Spam",
            relatedEntityId,
            $"Spam tipi: {spamType}, Tekrar sayısı: {spamCount}"
        );
    }

    /// <summary>
    /// Otomatik sistem uyarısı oluşturur
    /// </summary>
    public static Warning CreateAutomatic
    (
        string userId
        , string triggerRule
        , WarningType type
        , WarningSeverity severity
        , Dictionary<string, object>? metadata = null
    )
    {
        var reason = $"Otomatik tespit: {triggerRule}";
        var detailedExplanation = metadata != null
            ? JsonSerializer.Serialize(metadata)
            : null;

        var warning = Create(
            userId,
            "SYSTEM", // adminId olarak SYSTEM kullanıyoruz
            reason,
            type,
            severity,
            "AutoDetection",
            null,
            detailedExplanation,
            90 // Otomatik uyarılar 90 gün sonra sona erer
        );

        // Otomatik uyarı için ek event
        warning.AddDomainEvent(new AutomaticWarningIssuedEvent(
            warning.Id,
            userId,
            triggerRule,
            type.ToString(),
            severity.ToString(),
            metadata
        ));

        return warning;
    }

    /// <summary>
    /// Saygısız davranış uyarısı oluşturur
    /// </summary>
    public static Warning CreateForDisrespectfulBehavior
    (
        string userId
        , string adminId
        , string targetUserId
        , string incidentDescription
        , string? reviewId = null
    )
    {
        var reason = "Saygısız davranış ve hakaret";
        var detailedExplanation = $"Hedef kullanıcı: {targetUserId}. Olay: {incidentDescription}";

        return Create(
            userId,
            adminId,
            reason,
            WarningType.DisrespectfulBehavior,
            WarningSeverity.High, // Saygısızlık genellikle yüksek seviyeli uyarı
            "Review",
            reviewId,
            detailedExplanation
        );
    }

    /// <summary>
    /// Toplu uyarı oluşturur (birden fazla ihlal için)
    /// </summary>
    public static Warning CreateBulkWarning
    (
        string userId
        , string adminId
        , List<string> violations
        , WarningType type
        , WarningSeverity severity
    )
    {
        var reason = $"Çoklu ihlal tespit edildi ({violations.Count} adet)";
        var detailedExplanation = "İhlaller:\n" + string.Join("\n- ", violations);

        return Create(
            userId,
            adminId,
            reason,
            type,
            severity,
            "Multiple",
            null,
            detailedExplanation
        );
    }

    /// <summary>
    /// Uyarıyı onayla (kullanıcı gördü)
    /// </summary>
    public void Acknowledge()
    {
        if (IsAcknowledged)
            throw new BusinessRuleException("Uyarı zaten onaylanmış.");

        IsAcknowledged = true;
        AcknowledgedAt = DateTime.UtcNow;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new WarningAcknowledgedEvent(
            Id,
            UserId,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Uyarıya itiraz et
    /// </summary>
    public void Appeal(string appealNotes)
    {
        if (string.IsNullOrWhiteSpace(appealNotes))
            throw new ArgumentNullException(nameof(appealNotes));

        if (WasAppealed)
            throw new BusinessRuleException("Bu uyarıya zaten itiraz edilmiş.");

        if (!IsActive)
            throw new BusinessRuleException("Aktif olmayan uyarıya itiraz edilemez.");

        WasAppealed = true;
        AppealNotes = appealNotes;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new WarningAppealedEvent(
            Id,
            UserId,
            appealNotes,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Uyarı süresini kontrol et ve expire et
    /// </summary>
    public void CheckAndExpire()
    {
        if (!IsActive)
            return;

        if (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow)
        {
            IsActive = false;
            SetModifiedDate();

            // Domain Event
            AddDomainEvent(new WarningExpiredEvent(
                Id,
                UserId,
                DateTime.UtcNow
            ));
        }
    }

    /// <summary>
    /// Uyarıyı manuel olarak kaldır
    /// </summary>
    public void Remove(string removedBy, string reason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Uyarı zaten aktif değil.");

        IsActive = false;
        SetModifiedDate();
        // Note: Removal event can be added if needed
    }

    // Private helper methods
    private static int CalculatePoints(WarningSeverity severity)
    {
        return severity switch
        {
            WarningSeverity.Low => 1, WarningSeverity.Medium => 2, WarningSeverity.High => 3
            , WarningSeverity.Critical => 5, _ => 1
        };
    }

    // Validation methods
    private static void ValidateReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        if (reason.Length < 10)
            throw new BusinessRuleException("Uyarı nedeni en az 10 karakter olmalıdır.");

        if (reason.Length > 500)
            throw new BusinessRuleException("Uyarı nedeni 500 karakterden uzun olamaz.");
    }
}
