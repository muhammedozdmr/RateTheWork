using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;
// Uyarı: Bir kullanıcının aldığı uyarıları temsil eder.
public class Warning : BaseEntity
{
    public string UserId { get; set; } // Uyarıyı alan kullanıcı ID'si
    public string AdminId { get; set; } // Uyarıyı veren admin ID'si
    public string Reason { get; set; } // Uyarı nedeni
    public DateTime IssuedAt { get; set; }
    public string? RelatedCommentId { get; set; } // Hangi yorumla ilgili olduğu (isteğe bağlı)

    public Warning(string userId, string adminId, string reason)
    {
        UserId = userId;
        AdminId = adminId;
        Reason = reason;
        IssuedAt = DateTime.UtcNow;
        // BaseEntity constructor'ı otomatik çalışacak
    }
}
