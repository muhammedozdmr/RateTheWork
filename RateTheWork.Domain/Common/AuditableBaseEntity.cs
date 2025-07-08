namespace RateTheWork.Domain.Common;

/// <summary>
/// Audit bilgileri içeren entity'ler için temel sınıf.
/// Kim tarafından ve ne zaman oluşturuldu/güncellendi bilgilerini tutar.
/// </summary>
public abstract class AuditableBaseEntity : BaseEntity
{
    /// <summary>
    /// Entity'yi oluşturan kullanıcının ID'si
    /// </summary>
    public string? CreatedBy { get; protected set; }
    
    /// <summary>
    /// Entity'yi son güncelleyen kullanıcının ID'si
    /// </summary>
    public string? ModifiedBy { get; protected set; }
    
    /// <summary>
    /// Soft delete için silinme zamanı
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }
    
    /// <summary>
    /// Entity'yi silen kullanıcının ID'si
    /// </summary>
    public string? DeletedBy { get; protected set; }
    
    /// <summary>
    /// Soft delete durumu
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Yeni AuditableBaseEntity oluşturur
    /// </summary>
    protected AuditableBaseEntity() : base()
    {
        IsDeleted = false;
    }

    /// <summary>
    /// Varolan bir entity'yi yüklerken kullanılır
    /// </summary>
    protected AuditableBaseEntity(string id, DateTime createdAt, DateTime? modifiedAt, 
        string? createdBy, string? modifiedBy, DateTime? deletedAt, string? deletedBy, bool isDeleted) 
        : base(id, createdAt, modifiedAt)
    {
        CreatedBy = createdBy;
        ModifiedBy = modifiedBy;
        DeletedAt = deletedAt;
        DeletedBy = deletedBy;
        IsDeleted = isDeleted;
    }

    /// <summary>
    /// Entity oluşturulurken audit bilgilerini set eder
    /// </summary>
    public virtual void SetCreatedAudit(string userId)
    {
        CreatedBy = userId;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Entity güncellenirken audit bilgilerini set eder
    /// </summary>
    public virtual void SetModifiedAudit(string userId)
    {
        ModifiedBy = userId;
        SetModifiedDate();
    }

    /// <summary>
    /// Soft delete işlemi
    /// </summary>
    public virtual void SoftDelete(string userId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
        SetModifiedAudit(userId);
    }

    /// <summary>
    /// Soft delete'i geri al
    /// </summary>
    public virtual void Restore(string userId)
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        SetModifiedAudit(userId);
    }
}
