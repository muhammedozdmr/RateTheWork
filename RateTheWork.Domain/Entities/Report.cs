using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.Report;
using RateTheWork.Domain.Events.Report;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şikayet entity'si - Bir yorum hakkındaki şikayetleri temsil eder.
/// </summary>
public class Report : BaseEntity
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Report() : base()
    {
    }

    // Properties
    public string ReviewId { get; private set; } = string.Empty;
    public string ReporterUserId { get; private set; } = string.Empty;
    public ReportReasons ReportReason { get; private set; }
    public string? ReportDetails { get; private set; }
    public DateTime ReportedAt { get; private set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? AdminNotes { get; private set; }
    public string? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ActionTaken { get; private set; }
    public bool IsAnonymous { get; private set; }
    public int Priority { get; private set; }
    public bool RequiresUrgentAction { get; private set; }
    public string? RelatedReports { get; private set; }
    public string TargetType { get; private set; } = string.Empty;
    public string TargetId { get; private set; } = string.Empty;

    public Dictionary<string, object>? Metadata { get; private set; }

    /// <summary>
    /// Yeni şikayet oluşturur (Factory method)
    /// </summary>
    public static Report Create
    (
        string reviewId
        , string reporterUserId
        , string reportReason
        , string? reportDetails = null
        , bool isAnonymous = false
    )
    {
        // String'i enum'a çevir
        if (!Enum.TryParse<ReportReasons>(reportReason, true, out var reasonEnum))
        {
            throw new BusinessRuleException($"Geçersiz şikayet nedeni: {reportReason}");
        }

        return Create(reviewId, reporterUserId, reasonEnum, reportDetails, isAnonymous);
    }

    /// <summary>
    /// Yeni şikayet oluşturur (Factory method) - Enum parametre ile
    /// </summary>
    public static Report Create
    (
        string reviewId
        , string reporterUserId
        , ReportReasons reportReason
        , string? reportDetails = null
        , bool isAnonymous = false
    )
    {
        ValidateReportDetails(reportDetails);

        var priority = CalculatePriority(reportReason);
        var requiresUrgent = DetermineUrgency(reportReason);

        var report = new Report
        {
            ReviewId = reviewId ?? throw new ArgumentNullException(nameof(reviewId))
            , ReporterUserId = reporterUserId ?? throw new ArgumentNullException(nameof(reporterUserId))
            , ReportReason = reportReason, ReportDetails = reportDetails, ReportedAt = DateTime.UtcNow
            , Status = ReportStatus.Pending, IsAnonymous = isAnonymous, Priority = priority
            , RequiresUrgentAction = requiresUrgent, TargetType = "Review", TargetId = reviewId
        };

        // Domain Event
        report.AddDomainEvent(new ReportCreatedEvent(
            report.Id,
            reviewId,
            reporterUserId,
            reportReason.ToString(),
            isAnonymous,
            priority,
            report.ReportedAt
        ));

        return report;
    }

    /// <summary>
    /// Genel amaçlı şikayet oluşturma - targetType ve targetId parametreli
    /// </summary>
    public static Report Create
    (
        string reporterUserId
        , string targetType
        , string targetId
        , ReportReasons reportReason
        , string reportDetails
        , Dictionary<string, object>? metadata = null
    )
    {
        if (string.IsNullOrWhiteSpace(reporterUserId))
            throw new DomainValidationException(nameof(reporterUserId), "Reporter user ID zorunludur");

        if (string.IsNullOrWhiteSpace(targetType))
            throw new DomainValidationException(nameof(targetType), "Target type zorunludur");

        if (string.IsNullOrWhiteSpace(targetId))
            throw new DomainValidationException(nameof(targetId), "Target ID zorunludur");

        if (string.IsNullOrWhiteSpace(reportDetails))
            throw new DomainValidationException(nameof(reportDetails), "Rapor detayları zorunludur");

        // Hedef tipi kontrolü
        var validTargetTypes = new[] { "Review", "Company", "User" };
        if (!validTargetTypes.Contains(targetType))
            throw new DomainValidationException(nameof(targetType),
                $"Geçersiz hedef tipi. Geçerli tipler: {string.Join(", ", validTargetTypes)}");

        var priority = CalculatePriority(reportReason);
        var requiresUrgent = DetermineUrgency(reportReason);

        var report = new Report
        {
            Id = Guid.NewGuid().ToString(), ReporterUserId = reporterUserId, TargetType = targetType
            , TargetId = targetId, ReviewId = targetType == "Review" ? targetId : string.Empty
            , ReportReason = reportReason, ReportDetails = reportDetails, Status = ReportStatus.Pending
            , Metadata = metadata, CreatedAt = DateTime.UtcNow, ReportedAt = DateTime.UtcNow, Priority = priority
            , RequiresUrgentAction = requiresUrgent, IsAnonymous = reporterUserId == "SYSTEM"
        };

        // Domain event
        report.AddDomainEvent(new ReportCreatedEvent(
            report.Id,
            report.ReviewId,
            reporterUserId,
            reportReason.ToString(),
            report.IsAnonymous,
            priority,
            report.ReportedAt
        ));

        return report;
    }


    /// <summary>
    /// Şikayeti incelemeye al
    /// </summary>
    public void StartReview(string reviewedBy)
    {
        if (Status != ReportStatus.Pending)
            throw new BusinessRuleException("Sadece beklemedeki şikayetler incelemeye alınabilir.");

        Status = ReportStatus.UnderReview;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReportUnderReviewEvent(
            Id,
            reviewedBy,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Şikayeti çözümle
    /// </summary>
    public void Resolve(string resolvedBy, string actionTaken, string? adminNotes = null)
    {
        if (Status != ReportStatus.UnderReview)
            throw new BusinessRuleException("Sadece inceleme altındaki şikayetler çözümlenebilir.");

        ValidateActionTaken(actionTaken);

        Status = ReportStatus.Resolved;
        ActionTaken = actionTaken;
        AdminNotes = adminNotes;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReportResolvedEvent(
            Id,
            resolvedBy,
            actionTaken,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Şikayeti reddet
    /// </summary>
    public void Dismiss(string dismissedBy, string dismissReason)
    {
        if (Status == ReportStatus.Resolved || Status == ReportStatus.Dismissed)
            throw new BusinessRuleException("Çözümlenmiş veya reddedilmiş şikayet tekrar reddedilemez.");

        if (string.IsNullOrWhiteSpace(dismissReason))
            throw new ArgumentNullException(nameof(dismissReason));

        Status = ReportStatus.Dismissed;
        AdminNotes = dismissReason;
        ReviewedBy = dismissedBy;
        ReviewedAt = DateTime.UtcNow;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReportDismissedEvent(
            Id,
            dismissedBy,
            dismissReason,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Şikayeti üst yönetime ilet
    /// </summary>
    public void Escalate(string escalatedBy, string escalationReason)
    {
        if (Status != ReportStatus.UnderReview)
            throw new BusinessRuleException("Sadece inceleme altındaki şikayetler üst yönetime iletilebilir.");

        if (string.IsNullOrWhiteSpace(escalationReason))
            throw new ArgumentNullException(nameof(escalationReason));

        Status = ReportStatus.Escalated;
        AdminNotes = $"Escalation: {escalationReason}";
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReportEscalatedEvent(
            Id,
            escalatedBy,
            escalationReason,
            DateTime.UtcNow
        ));
    }

    // Private helper methods
    private static int CalculatePriority(ReportReasons reportReason)
    {
        return reportReason switch
        {
            ReportReasons.Harassment => 1, ReportReasons.PersonalAttack => 1, ReportReasons.ConfidentialInfo => 2
            , ReportReasons.FalseInformation => 3, ReportReasons.InappropriateContent => 3, ReportReasons.Spam => 4
            , ReportReasons.OffTopic => 5, ReportReasons.Duplicate => 5, ReportReasons.HighDownvoteRatio => 4, _ => 6
        };
    }


    private static bool DetermineUrgency(ReportReasons reportReason)
    {
        return reportReason == ReportReasons.Harassment ||
               reportReason == ReportReasons.PersonalAttack ||
               reportReason == ReportReasons.ConfidentialInfo;
    }

    private static void ValidateReportDetails(string? reportDetails)
    {
        if (!string.IsNullOrWhiteSpace(reportDetails) && reportDetails.Length > 1000)
            throw new BusinessRuleException("Şikayet detayı 1000 karakterden uzun olamaz.");
    }

    private static void ValidateActionTaken(string actionTaken)
    {
        if (string.IsNullOrWhiteSpace(actionTaken))
            throw new ArgumentNullException(nameof(actionTaken));

        var validActions = new[]
        {
            "Yorum Gizlendi", "Kullanıcı Uyarıldı", "Kullanıcı Banlandı", "İçerik Düzenlendi"
            , "Herhangi Bir İşlem Yapılmadı", "Üst Yönetime İletildi"
        };

        if (!validActions.Contains(actionTaken))
            throw new BusinessRuleException("Geçersiz aksiyon türü.");
    }

    /// <summary>
    /// Metadata ekler veya günceller
    /// </summary>
    public void AddMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
        SetModifiedDate();
    }

    /// <summary>
    /// Raporun sistem tarafından oluşturulup oluşturulmadığını kontrol eder
    /// </summary>
    public bool IsSystemGenerated()
    {
        return ReporterUserId == "SYSTEM" ||
               (Metadata?.ContainsKey("IsSystemGenerated") == true &&
                Metadata["IsSystemGenerated"] is bool isSystem && isSystem);
    }

    /// <summary>
    /// Raporun acil olup olmadığını kontrol eder
    /// </summary>
    public bool IsUrgent()
    {
        return RequiresUrgentAction;
    }
}
