using RateTheWork.Domain.Entities.Common;

namespace RateTheWork.Domain.Entities;

// Denetim Kaydı: Admin panelinde yapılan kritik işlemlerin izlenmesi için.
public class AuditLog : BaseEntity
{
    public string AdminUserId { get; set; } // İşlemi yapan adminin ID'si
    public string ActionType { get; set; } // İşlem türü (örn: "ReviewApproved", "UserBanned", "CompanyDeleted")
    public string EntityType { get; set; } // Etkilenen varlık türü (örn: "Review", "User", "Company")
    public string EntityId { get; set; } // Etkilenen varlığın ID'si
    public string? Details { get; set; } // İşlemle ilgili ek detaylar (örn: reddetme nedeni)
    public DateTime Timestamp { get; set; }

    public AuditLog(string adminUserId, string actionType, string entityType, string entityId)
    {
        AdminUserId = adminUserId;
        ActionType = actionType;
        EntityType = entityType;
        EntityId = entityId;
        Timestamp = DateTime.UtcNow;
        // BaseEntity constructor'ı otomatik çalışacak
    }
}
