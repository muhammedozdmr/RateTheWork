using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Events.Report;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şikayet entity'si - Bir yorum hakkındaki şikayetleri temsil eder.
/// </summary>
public class Report : BaseEntity
{
    // Report Reasons
    public static class ReportReasons
    {
        public const string InappropriateContent = "Uygunsuz İçerik";
        public const string FalseInformation = "Yanlış/Yanıltıcı Bilgi";
        public const string Spam = "Spam";
        public const string Harassment = "Taciz/Zorbalık";
        public const string PersonalAttack = "Kişisel Saldırı";
        public const string ConfidentialInfo = "Gizli Bilgi İçeriyor";
        public const string OffTopic = "Konu Dışı";
        public const string Duplicate = "Mükerrer İçerik";
        public const string Other = "Diğer";
    }

    // Report Status
    public static class ReportStatuses
    {
        public const string Pending = "Beklemede";
        public const string UnderReview = "İnceleniyor";
        public const string Resolved = "Çözümlendi";
        public const string Dismissed = "Reddedildi";
        public const string Escalated = "Üst Yönetime İletildi";
    }

    // Properties
    public string ReviewId { get; private set; } = string.Empty;
    public string ReporterUserId { get; private set; } = string.Empty;
    public string ReportReason { get; private set; } = string.Empty;
    public string? ReportDetails { get; private set; }
    public DateTime ReportedAt { get; private set; }
    public string Status { get; private set; } = ReportStatuses.Pending;
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

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Report() : base()
    {
    }

    /// <summary>
    /// Yeni şikayet oluşturur (Factory method)
    /// </summary>
    public static Report Create(
        string reviewId,
        string reporterUserId,
        string reportReason,
        string? reportDetails = null,
        bool isAnonymous = false)
    {
        ValidateReportReason(reportReason);
        ValidateReportDetails(reportDetails);

        var priority = CalculatePriority(reportReason);
        var requiresUrgent = DetermineUrgency(reportReason);

        var report = new Report
        {
            ReviewId = reviewId ?? throw new ArgumentNullException(nameof(reviewId)),
            ReporterUserId = reporterUserId ?? throw new ArgumentNullException(nameof(reporterUserId)),
            ReportReason = reportReason,
            ReportDetails = reportDetails,
            ReportedAt = DateTime.UtcNow,
            Status = ReportStatuses.Pending,
            IsAnonymous = isAnonymous,
            Priority = priority,
            RequiresUrgentAction = requiresUrgent
        };

        // Domain Event
        report.AddDomainEvent(new ReportCreatedEvent(
            report.Id,
            reviewId,
            reporterUserId,
            reportReason,
            isAnonymous,
            priority,
            report.ReportedAt,
            DateTime.UtcNow
        ));

        return report;
    }

    /// <summary>
    /// Şikayeti incelemeye al
    /// </summary>
    public void StartReview(string reviewedBy)
    {
        if (Status != ReportStatuses.Pending)
            throw new BusinessRuleException("Sadece beklemedeki şikayetler incelemeye alınabilir.");

        Status = ReportStatuses.UnderReview;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReportUnderReviewEvent(
            Id,
            reviewedBy,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Şikayeti çözümle
    /// </summary>
    public void Resolve(string resolvedBy, string actionTaken, string? adminNotes = null)
    {
        if (Status != ReportStatuses.UnderReview)
            throw new BusinessRuleException("Sadece inceleme altındaki şikayetler çözümlenebilir.");

        ValidateActionTaken(actionTaken);

        Status = ReportStatuses.Resolved;
        ActionTaken = actionTaken;
        AdminNotes = adminNotes;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReportResolvedEvent(
            Id,
            resolvedBy,
            actionTaken,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Şikayeti reddet
    /// </summary>
    public void Dismiss(string dismissedBy, string dismissReason)
    {
        if (Status == ReportStatuses.Resolved || Status == ReportStatuses.Dismissed)
            throw new BusinessRuleException("Çözümlenmiş veya reddedilmiş şikayet tekrar reddedilemez.");

        if (string.IsNullOrWhiteSpace(dismissReason))
            throw new ArgumentNullException(nameof(dismissReason));

        Status = ReportStatuses.Dismissed;
        AdminNotes = dismissReason;
        ReviewedBy = dismissedBy;
        ReviewedAt = DateTime.UtcNow;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReportDismissedEvent(
            Id,
            dismissedBy,
            dismissReason,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Şikayeti üst yönetime ilet
    /// </summary>
    public void Escalate(string escalatedBy, string escalationReason)
    {
        if (Status != ReportStatuses.UnderReview)
            throw new BusinessRuleException("Sadece inceleme altındaki şikayetler üst yönetime iletilebilir.");

        if (string.IsNullOrWhiteSpace(escalationReason))
            throw new ArgumentNullException(nameof(escalationReason));

        Status = ReportStatuses.Escalated;
        AdminNotes = $"Escalation: {escalationReason}";
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReportEscalatedEvent(
            Id,
            escalatedBy,
            escalationReason,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    // Private helper methods
    private static int CalculatePriority(string reportReason)
    {
        return reportReason switch
        {
            ReportReasons.Harassment => 5,
            ReportReasons.PersonalAttack => 5,
            ReportReasons.ConfidentialInfo => 4,
            ReportReasons.FalseInformation => 3,
            ReportReasons.InappropriateContent => 3,
            ReportReasons.Spam => 2,
            ReportReasons.OffTopic => 1,
            ReportReasons.Duplicate => 1,
            _ => 2
        };
    }

    private static bool DetermineUrgency(string reportReason)
    {
        return reportReason == ReportReasons.Harassment ||
               reportReason == ReportReasons.PersonalAttack ||
               reportReason == ReportReasons.ConfidentialInfo;
    }

    // Validation methods
    private static void ValidateReportReason(string reportReason)
    {
        if (string.IsNullOrWhiteSpace(reportReason))
            throw new ArgumentNullException(nameof(reportReason));

        var validReasons = typeof(ReportReasons)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Select(f => f.GetValue(null)?.ToString())
            .Where(v => v != null)
            .ToList();

        if (!validReasons.Contains(reportReason))
            throw new BusinessRuleException("Geçersiz şikayet nedeni.");
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

        if (actionTaken.Length < 10)
            throw new BusinessRuleException("Alınan aksiyon açıklaması en az 10 karakter olmalıdır.");
    }
}