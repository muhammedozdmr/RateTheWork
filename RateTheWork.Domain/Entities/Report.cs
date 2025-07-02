using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şikayet entity'si - Bir yorum hakkındaki şikayetleri temsil eder
/// </summary>
public class Report : BaseEntity
{
    /// <summary>
    /// Şikayet edilen yorumun ID'si
    /// </summary>
    public string ReviewId { get; set; }
    
    /// <summary>
    /// Şikayet eden kullanıcının ID'si
    /// </summary>
    public string ReporterUserId { get; set; }
    
    /// <summary>
    /// Şikayet nedeni (örn: "Ahlaksız İçerik", "Yanlış Bilgi", "Spam")
    /// </summary>
    public string ReportReason { get; set; }
    
    /// <summary>
    /// Şikayet detayları (opsiyonel)
    /// </summary>
    public string? ReportDetails { get; set; }
    
    /// <summary>
    /// Şikayet tarihi
    /// </summary>
    public DateTime ReportedAt { get; set; }
    
    /// <summary>
    /// Şikayet durumu: "Pending", "Reviewed", "ActionTaken", "Dismissed"
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Admin'in şikayetle ilgili notları
    /// </summary>
    public string? AdminNotes { get; set; }

    /// <summary>
    /// Report constructor
    /// </summary>
    public Report(string reviewId, string reporterUserId, string reportReason)
    {
        ReviewId = reviewId;
        ReporterUserId = reporterUserId;
        ReportReason = reportReason;
        ReportedAt = DateTime.UtcNow;
        Status = "Pending";
        // BaseEntity constructor'ı otomatik çalışacak
    }
}

