namespace RateTheWork.Domain.Enums;

public enum NotificationType
{
    CommentReply,          // Yoruma yanıt
    DocumentApproved,      // Belge onaylandı
    DocumentRejected,      // Belge reddedildi
    WarningIssued,         // Uyarı verildi
    BadgeEarned,           // Rozet kazanıldı
    CompanyApproved,       // Şirket onaylandı
    CompanyRejected,       // Şirket reddedildi
    ReviewReported,        // Yorum şikayet edildi
    System                 // Sistem bildirimi
}
