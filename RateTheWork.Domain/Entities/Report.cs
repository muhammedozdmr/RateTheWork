using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.Report;
using RateTheWork.Domain.Events.Report;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şikayet/İhbar entity'si - Yorum veya kullanıcılar hakkındaki şikayetleri tutar
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
    public string ReportedEntityType { get; private set; } = string.Empty; // Review, User, Company
    public string ReportedEntityId { get; private set; } = string.Empty;
    public string ReporterUserId { get; private set; } = string.Empty;
    public ReportReason Reason { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public ReportStatus Status { get; private set; } = ReportStatus.Pending;
    public string? ReviewerUserId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewerNotes { get; private set; }
    public ReportResolution? Resolution { get; private set; }
    public List<string> EvidenceUrls { get; private set; } = new();
    public bool IsAnonymous { get; private set; }

    // Eski kodlarla uyumluluk için alias'lar
    public string EntityType => ReportedEntityType;
    public string EntityId => ReportedEntityId;
    public string TargetType => ReportedEntityType;
    public string TargetId => ReportedEntityId;
    public string ReviewId => ReportedEntityType == "Review" ? ReportedEntityId : string.Empty;
    public string ReportReason => Reason.ToString();
    public string ReportDetails => Description;
    public DateTime ReportedAt => CreatedAt;

    public int Priority => Reason switch
    {
        Enums.Report.ReportReason.HateSpeech or Enums.Report.ReportReason.Harassment => 5
        , Enums.Report.ReportReason.OffensiveContent => 4, Enums.Report.ReportReason.Misinformation => 3
        , Enums.Report.ReportReason.Spam => 2, _ => 1
    };

    public bool RequiresUrgentAction => Priority >= 4;

    /// <summary>
    /// Yeni şikayet oluşturur (Factory method)
    /// </summary>
    public static Report Create
    (
        string reportedEntityType
        , string reportedEntityId
        , string reporterUserId
        , ReportReason reason
        , string description
        , bool isAnonymous = false
        , List<string>? evidenceUrls = null
    )
    {
        // Validasyonlar
        if (string.IsNullOrWhiteSpace(reportedEntityType))
            throw new ArgumentNullException(nameof(reportedEntityType));

        if (string.IsNullOrWhiteSpace(reportedEntityId))
            throw new ArgumentNullException(nameof(reportedEntityId));

        if (string.IsNullOrWhiteSpace(reporterUserId))
            throw new ArgumentNullException(nameof(reporterUserId));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentNullException(nameof(description));

        if (description.Length < 10)
            throw new BusinessRuleException("Şikayet açıklaması en az 10 karakter olmalıdır.");

        if (description.Length > 1000)
            throw new BusinessRuleException("Şikayet açıklaması 1000 karakterden uzun olamaz.");

        var validEntityTypes = new[] { "Review", "User", "Company" };
        if (!validEntityTypes.Contains(reportedEntityType))
            throw new BusinessRuleException($"Geçersiz entity tipi: {reportedEntityType}");

        var report = new Report
        {
            ReportedEntityType = reportedEntityType, ReportedEntityId = reportedEntityId
            , ReporterUserId = reporterUserId, Reason = reason, Description = description, IsAnonymous = isAnonymous
            , EvidenceUrls = evidenceUrls ?? new List<string>()
        };

        // Domain event
        report.AddDomainEvent(new ReportCreatedEvent(
            report.Id,
            reportedEntityId,
            reporterUserId,
            reason.ToString(),
            isAnonymous,
            reason == Enums.Report.ReportReason.HateSpeech || reason == Enums.Report.ReportReason.Harassment ? 1 : 2,
            report.CreatedAt
        ));

        return report;
    }

    /// <summary>
    /// Şikayeti incele ve sonuçlandır
    /// </summary>
    public void Review(string reviewerUserId, ReportResolution resolution, string? notes = null)
    {
        if (Status != ReportStatus.Pending)
            throw new BusinessRuleException("Sadece bekleyen şikayetler incelenebilir.");

        ReviewerUserId = reviewerUserId;
        ReviewedAt = DateTime.UtcNow;
        Resolution = resolution;
        ReviewerNotes = notes;

        Status = resolution switch
        {
            ReportResolution.Approved => ReportStatus.Resolved, ReportResolution.Rejected => ReportStatus.Rejected
            , ReportResolution.NeedsMoreInfo => ReportStatus.InReview, _ => ReportStatus.Resolved
        };

        SetModifiedDate();

        // Domain event
        if (resolution == ReportResolution.Approved)
        {
            AddDomainEvent(new ReportResolvedEvent(
                Id,
                reviewerUserId,
                resolution.ToString(),
                ReviewedAt.Value
            ));
        }
        else if (resolution == ReportResolution.Rejected)
        {
            AddDomainEvent(new ReportDismissedEvent(
                Id,
                reviewerUserId,
                notes ?? "Şikayet haksız bulundu",
                ReviewedAt.Value
            ));
        }
    }

    /// <summary>
    /// Şikayeti reddet
    /// </summary>
    public void Reject(string reviewerUserId, string notes)
    {
        if (Status == ReportStatus.Resolved || Status == ReportStatus.Rejected)
            throw new BusinessRuleException("Bu şikayet zaten işlenmiş.");

        Status = ReportStatus.Rejected;
        ReviewerUserId = reviewerUserId;
        ReviewedAt = DateTime.UtcNow;
        ReviewerNotes = notes;
        SetModifiedDate();

        AddDomainEvent(new ReportDismissedEvent(
            Id,
            reviewerUserId,
            notes,
            ReviewedAt.Value
        ));
    }

    /// <summary>
    /// Şikayete ek kanıt ekle
    /// </summary>
    public void AddEvidence(string evidenceUrl)
    {
        if (Status == ReportStatus.Resolved || Status == ReportStatus.Rejected)
            throw new BusinessRuleException("Kapatılmış şikayetlere kanıt eklenemez.");

        if (EvidenceUrls.Count >= 5)
            throw new BusinessRuleException("En fazla 5 kanıt eklenebilir.");

        EvidenceUrls.Add(evidenceUrl);
        SetModifiedDate();
    }

    /// <summary>
    /// Şikayeti tekrar aç
    /// </summary>
    public void Reopen(string reason)
    {
        if (Status != ReportStatus.Resolved && Status != ReportStatus.Rejected)
            throw new BusinessRuleException("Sadece kapatılmış şikayetler tekrar açılabilir.");

        Status = ReportStatus.InReview;
        ReviewerNotes = $"{ReviewerNotes}\n\nTekrar açılma nedeni: {reason}";
        SetModifiedDate();
    }
}
