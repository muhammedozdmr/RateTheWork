using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
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
    public string? ReviewId { get; private set; }
    public string? ReporterUserId { get; private set; }
    public string? ReportReason { get; private set; }
    public string? ReportDetails { get; private set; }
    public DateTime ReportedAt { get; private set; }
    public string? Status { get; private set; }
    public string? AdminNotes { get; private set; }
    public string? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ActionTaken { get; private set; } // Alınan aksiyon
    public bool IsAnonymous { get; private set; } // Anonim şikayet mi?
    public int Priority { get; private set; } // Öncelik (1-5)
    public bool RequiresUrgentAction { get; private set; }
    public string? RelatedReports { get; private set; } // İlişkili diğer şikayetler

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Report() : base()
    {
    }
    
    /// <summary>
    /// EF Core için private constructor
    /// </summary>
    private Report(string? reviewId, string? reporterUserId, string? reportReason, string? status) : base()
    {
        ReviewId = reviewId;
        ReporterUserId = reporterUserId;
        ReportReason = reportReason;
        Status = status;
    }

    /// <summary>
    /// Yeni şikayet oluşturur
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

        var report = new Report
        {
            ReviewId = reviewId ?? throw new ArgumentNullException(nameof(reviewId)),
            ReporterUserId = reporterUserId ?? throw new ArgumentNullException(nameof(reporterUserId)),
            ReportReason = reportReason,
            ReportDetails = reportDetails,
            ReportedAt = DateTime.UtcNow,
            Status = ReportStatuses.Pending,
            IsAnonymous = isAnonymous,
            Priority = CalculatePriority(reportReason),
            RequiresUrgentAction = false
        };

        // Belirli durumlar için acil işlem gerekir
        if (reportReason == ReportReasons.Harassment || 
            reportReason == ReportReasons.PersonalAttack ||
            reportReason == ReportReasons.ConfidentialInfo)
        {
            report.RequiresUrgentAction = true;
            report.Priority = 5; // Maksimum öncelik
        }

        // Domain Event
        report.AddDomainEvent(new ReportCreatedEvent(
            report.Id,
            reviewId,
            reporterUserId,
            reportReason,
            report.RequiresUrgentAction
        ));

        return report;
    }

    /// <summary>
    /// Şikayeti incelemeye al
    /// </summary>
    public void StartReview(string adminId)
    {
        if (Status != ReportStatuses.Pending)
            throw new BusinessRuleException($"Sadece '{ReportStatuses.Pending}' durumundaki şikayetler incelemeye alınabilir.");

        Status = ReportStatuses.UnderReview;
        ReviewedBy = adminId;
        ReviewedAt = DateTime.UtcNow;
        SetModifiedDate();

        AddDomainEvent(new ReportReviewStartedEvent(Id, ReviewId, adminId));
    }

    /// <summary>
    /// Şikayeti çözümle
    /// </summary>
    public void Resolve(string adminId, string actionTaken, string? adminNotes = null)
    {
        if (Status != ReportStatuses.UnderReview)
            throw new BusinessRuleException("Sadece inceleme altındaki şikayetler çözümlenebilir.");

        if (string.IsNullOrWhiteSpace(actionTaken))
            throw new ArgumentNullException(nameof(actionTaken));

        Status = ReportStatuses.Resolved;
        ActionTaken = actionTaken;
        AdminNotes = CombineNotes(AdminNotes, adminNotes, adminId);
        
        if (string.IsNullOrEmpty(ReviewedBy))
        {
            ReviewedBy = adminId;
            ReviewedAt = DateTime.UtcNow;
        }
        
        SetModifiedDate();

        AddDomainEvent(new ReportResolvedEvent(
            Id,
            ReviewId,
            adminId,
            actionTaken,
            ReportReason
        ));
    }

    /// <summary>
    /// Şikayeti reddet
    /// </summary>
    public void Dismiss(string adminId, string dismissReason)
    {
        if (Status == ReportStatuses.Resolved || Status == ReportStatuses.Dismissed)
            throw new BusinessRuleException("Çözümlenmiş veya reddedilmiş şikayet tekrar reddedilemez.");

        if (string.IsNullOrWhiteSpace(dismissReason))
            throw new ArgumentNullException(nameof(dismissReason));

        Status = ReportStatuses.Dismissed;
        ActionTaken = "Şikayet reddedildi";
        AdminNotes = CombineNotes(AdminNotes, $"Red nedeni: {dismissReason}", adminId);
        
        if (string.IsNullOrEmpty(ReviewedBy))
        {
            ReviewedBy = adminId;
            ReviewedAt = DateTime.UtcNow;
        }
        
        SetModifiedDate();

        AddDomainEvent(new ReportDismissedEvent(Id, ReviewId, adminId, dismissReason));
    }

    /// <summary>
    /// Şikayeti üst yönetime ilet
    /// </summary>
    public void Escalate(string adminId, string escalationReason)
    {
        if (Status == ReportStatuses.Resolved || Status == ReportStatuses.Dismissed)
            throw new BusinessRuleException("Çözümlenmiş veya reddedilmiş şikayet üst yönetime iletilemez.");

        if (string.IsNullOrWhiteSpace(escalationReason))
            throw new ArgumentNullException(nameof(escalationReason));

        Status = ReportStatuses.Escalated;
        Priority = 5; // Maksimum öncelik
        RequiresUrgentAction = true;
        AdminNotes = CombineNotes(AdminNotes, $"Üst yönetime iletme nedeni: {escalationReason}", adminId);
        SetModifiedDate();

        AddDomainEvent(new ReportEscalatedEvent(Id, ReviewId, adminId, escalationReason));
    }

    /// <summary>
    /// Admin notu ekle
    /// </summary>
    public void AddAdminNote(string adminId, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentNullException(nameof(note));

        AdminNotes = CombineNotes(AdminNotes, note, adminId);
        SetModifiedDate();
    }

    /// <summary>
    /// İlişkili şikayet ekle
    /// </summary>
    public void LinkRelatedReport(string relatedReportId)
    {
        if (Id == relatedReportId)
            throw new BusinessRuleException("Şikayet kendisiyle ilişkilendirilemez.");

        if (string.IsNullOrWhiteSpace(RelatedReports))
        {
            RelatedReports = relatedReportId;
        }
        else
        {
            var reports = RelatedReports.Split(',').ToList();
            if (!reports.Contains(relatedReportId))
            {
                reports.Add(relatedReportId);
                RelatedReports = string.Join(",", reports);
            }
        }
        
        SetModifiedDate();
    }

    /// <summary>
    /// Önceliği güncelle
    /// </summary>
    public void UpdatePriority(int newPriority, string updatedBy, string reason)
    {
        if (newPriority < 1 || newPriority > 5)
            throw new BusinessRuleException("Öncelik 1-5 arasında olmalıdır.");

        if (Priority == newPriority)
            return;

        var oldPriority = Priority;
        Priority = newPriority;
        AdminNotes = CombineNotes(AdminNotes, $"Öncelik değiştirildi: {oldPriority} -> {newPriority}. Neden: {reason}", updatedBy);
        SetModifiedDate();
    }

    /// <summary>
    /// Acil işlem durumunu güncelle
    /// </summary>
    public void SetUrgentAction(bool isUrgent, string setBy, string reason)
    {
        if (RequiresUrgentAction == isUrgent)
            return;

        RequiresUrgentAction = isUrgent;
        if (isUrgent)
        {
            Priority = Math.Max(Priority, 4); // En az 4 öncelik
        }
        
        AdminNotes = CombineNotes(AdminNotes, 
            $"Acil işlem durumu: {(isUrgent ? "EVET" : "HAYIR")}. Neden: {reason}", 
            setBy);
        SetModifiedDate();
    }

    /// <summary>
    /// Şikayet özetini döndür
    /// </summary>
    public string GetSummary()
    {
        var summary = $"{ReportReason}";
        
        if (!string.IsNullOrWhiteSpace(ReportDetails))
        {
            var preview = ReportDetails.Length > 50 
                ? ReportDetails.Substring(0, 50) + "..." 
                : ReportDetails;
            summary += $": {preview}";
        }

        summary += $" [{Status}]";
        
        if (RequiresUrgentAction)
            summary = "🚨 " + summary;
            
        return summary;
    }

    /// <summary>
    /// İşlem geçmişini döndür
    /// </summary>
    public string GetActionHistory()
    {
        var history = $"Şikayet Tarihi: {ReportedAt:dd.MM.yyyy HH:mm}\n";
        history += $"Durum: {Status}\n";
        
        if (ReviewedAt.HasValue)
        {
            history += $"İnceleme Tarihi: {ReviewedAt.Value:dd.MM.yyyy HH:mm}\n";
            history += $"İnceleyen: {ReviewedBy}\n";
        }
        
        if (!string.IsNullOrWhiteSpace(ActionTaken))
        {
            history += $"Alınan Aksiyon: {ActionTaken}\n";
        }
        
        return history;
    }

    // Private helper methods
    private static int CalculatePriority(string reportReason)
    {
        return reportReason switch
        {
            ReportReasons.Harassment => 5,
            ReportReasons.PersonalAttack => 5,
            ReportReasons.ConfidentialInfo => 5,
            ReportReasons.InappropriateContent => 4,
            ReportReasons.FalseInformation => 3,
            ReportReasons.Spam => 2,
            ReportReasons.OffTopic => 1,
            ReportReasons.Duplicate => 1,
            _ => 3
        };
    }

    private static string CombineNotes(string? existingNotes, string? newNote, string adminId)
    {
        if (string.IsNullOrWhiteSpace(newNote))
            return existingNotes ?? string.Empty;

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var formattedNote = $"[{timestamp}] {adminId}: {newNote}";

        return string.IsNullOrWhiteSpace(existingNotes)
            ? formattedNote
            : $"{existingNotes}\n{formattedNote}";
    }

    // Validation methods
    private static void ValidateReportReason(string reason)
    {
        var validReasons = new[]
        {
            ReportReasons.InappropriateContent,
            ReportReasons.FalseInformation,
            ReportReasons.Spam,
            ReportReasons.Harassment,
            ReportReasons.PersonalAttack,
            ReportReasons.ConfidentialInfo,
            ReportReasons.OffTopic,
            ReportReasons.Duplicate,
            ReportReasons.Other
        };

        if (!validReasons.Contains(reason))
            throw new BusinessRuleException($"Geçersiz şikayet nedeni: {reason}");
    }

    private static void ValidateReportDetails(string? details)
    {
        if (!string.IsNullOrWhiteSpace(details) && details.Length > 1000)
            throw new BusinessRuleException("Şikayet detayları 1000 karakterden fazla olamaz.");
    }
}