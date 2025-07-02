using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

// Ban: Bir kullanıcının banlanma kaydını temsil eder.
public class Ban : BaseEntity
{
    public string UserId { get; set; } // Banlanan kullanıcı ID'si
    public string AdminId { get; set; } // Banı veren admin ID'si
    public string Reason { get; set; } // Ban nedeni
    public DateTime BannedAt { get; set; }
    public DateTime? UnbanDate { get; set; } // Banın kaldırılacağı tarih (süresizse null)
    public bool IsActive { get; set; } // Ban hala aktif mi?

    public Ban(string userId, string adminId, string reason)
    {
        UserId = userId;
        AdminId = adminId;
        Reason = reason;
        BannedAt = DateTime.UtcNow;
        IsActive = true;
        // BaseEntity constructor'ı otomatik çalışacak
    }
}
