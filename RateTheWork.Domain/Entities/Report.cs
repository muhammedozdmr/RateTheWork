using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

// Şikayet: Bir yorum hakkındaki şikayetleri temsil eder.
public class Report : BaseEntity
{
    public string CommentId { get; set; } // Şikayet edilen yorumun ID'si
    public string ReporterUserId { get; set; } // Şikayet eden kullanıcı ID'si
    public string ReportReason { get; set; } // Şikayet nedeni (örn: "Ahlaksız İçerik", "Yanlış Bilgi")
    public string? ReportDetails { get; set; } // Şikayet detayları (isteğe bağlı)
    public DateTime ReportedAt { get; set; }
    public string Status { get; set; } // "Pending", "Reviewed", "ActionTaken"
    public string? AdminNotes { get; set; } // Adminin şikayetle ilgili notları

    public Report(string commentId, string reporterUserId, string reportReason)
    {
        CommentId = commentId; 
        ReporterUserId = reporterUserId;
        ReportReason = reportReason;
        ReportedAt = DateTime.UtcNow;
        Status = "Pending";
        // BaseEntity constructor'ı otomatik çalışacak
    }
}

