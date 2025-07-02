using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

// Bildirim: Kullanıcılara gönderilen her bir bildirimi kaydetmek için.
public class Notification : BaseEntity
{
    public string UserId { get; set; } // Bildirimin alıcısı kullanıcı ID'si
    public string Type { get; set; } // Bildirim türü (örn: "CommentReply", "DocumentApproved", "WarningIssued")
    public string Message { get; set; } // Bildirim mesajı
    public bool IsRead { get; set; } = false; // Kullanıcı tarafından okundu mu?
    public string? RelatedEntityId { get; set; } // İlgili varlığın ID'si (örn: yorum ID'si, şirket ID'si)
    public string? RelatedEntityType { get; set; } // İlgili varlığın türü (örn: "Review", "Company")

    public Notification(string userId, string type, string message)
    {
        UserId = userId;
        Type = type;
        Message = message;
        // BaseEntity constructor'ı otomatik çalışacak
    }
}
