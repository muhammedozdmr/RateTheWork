namespace RateTheWork.Domain.Entities.Common;

// Audit edilebilir entity'ler için (admin işlemleri vs.)
public abstract class AuditableBaseEntity : BaseEntity
{
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; } = false; // Soft delete için
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

