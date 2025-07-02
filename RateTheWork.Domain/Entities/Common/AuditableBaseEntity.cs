namespace RateTheWork.Domain.Entities.Common;

/// <summary>
/// Audit edilebilir entity'ler için (kim tarafından oluşturuldu/güncellendi bilgisi)
/// </summary>
public abstract class AuditableBaseEntity : BaseEntity
{
    /// <summary>
    /// Entity'yi oluşturan kullanıcının ID'si
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// Entity'yi son güncelleyen kullanıcının ID'si
    /// </summary>
    public string? ModifiedBy { get; set; }
    
    /// <summary>
    /// Soft delete için - entity silinmiş mi?
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Silinme tarihi (soft delete)
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Entity'yi silen kullanıcının ID'si
    /// </summary>
    public string? DeletedBy { get; set; }
}

