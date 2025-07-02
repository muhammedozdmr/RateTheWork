using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

// Kullanıcı Rozeti: Hangi kullanıcının hangi rozeti kazandığını eşleştirmek için.
public class UserBadge : BaseEntity
{
    public string UserId { get; set; } // Rozeti kazanan kullanıcı ID'si
    public string BadgeId { get; set; } // Kazanılan rozet ID'si
    public DateTime AwardedAt { get; set; } // Rozetin kazanıldığı tarih

    public UserBadge(string userId, string badgeId)
    {
        UserId = userId;
        BadgeId = badgeId;
        AwardedAt = DateTime.UtcNow;
        // BaseEntity constructor'ı otomatik çalışacak
    }
}