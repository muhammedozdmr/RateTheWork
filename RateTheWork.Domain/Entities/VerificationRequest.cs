using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

// Doğrulama Talebi: Admin onayı bekleyen bilgi doğrulamalarını temsil eder.
public class VerificationRequest : ApprovableBaseEntity
{
    public string ReviewId { get; set; } // Hangi yorumun doğrulanması isteniyor
    public string UserId { get; set; } // Talebi oluşturan kullanıcı
    public string AdminId { get; set; } // Talebi onaylayan/reddeden admin (null olabilir)
    public string DocumentUrl { get; set; } // Doğrulama için yüklenen belgenin URL'si
    public string DocumentName { get; set; } // Belge adı
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } // "Pending", "Approved", "Rejected"
    // AdminNotes artık ApprovableBaseEntity'deki ApprovalNotes olarak kullanılacak

    public VerificationRequest(string reviewId, string userId, string adminId, string documentUrl, string documentName)
    {
        ReviewId = reviewId;
        UserId = userId;
        AdminId = adminId;
        DocumentUrl = documentUrl;
        DocumentName = documentName;
        RequestedAt = DateTime.UtcNow;
        Status = "Pending"; // Varsayılan durum
        // BaseEntity constructor'ı otomatik çalışacak
    }
}
