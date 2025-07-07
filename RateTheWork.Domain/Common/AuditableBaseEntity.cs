namespace RateTheWork.Domain.Common;

/// <summary>
/// Audit edilebilir entity'ler için base class.
/// Kim tarafından oluşturuldu/güncellendi bilgisini tutar.
/// Soft delete desteği içerir.
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
    public bool IsDeleted { get; protected set; }
    
    /// <summary>
    /// Silinme tarihi (soft delete)
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }
    
    /// <summary>
    /// Entity'yi silen kullanıcının ID'si
    /// </summary>
    public string? DeletedBy { get; protected set; }

    /// <summary>
    /// Yeni auditable entity oluşturur
    /// </summary>
    protected AuditableBaseEntity() : base()
    {
        IsDeleted = false;
    }

    /// <summary>
    /// Varolan bir auditable entity'yi yüklerken kullanılır
    /// </summary>
    protected AuditableBaseEntity(string? id, DateTime createdAt, DateTime? modifiedAt) 
        : base(id, createdAt, modifiedAt)
    {
        IsDeleted = false;
    }

    /// <summary>
    /// Entity'yi soft delete yapar
    /// </summary>
    /// <param name="deletedBy">Silen kullanıcının ID'si</param>
    public virtual void Delete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        SetModifiedDate();
    }

    /// <summary>
    /// Soft delete'i geri alır
    /// </summary>
    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        SetModifiedDate();
    }

    /// <summary>
    /// Entity'yi güncellerken audit bilgilerini set eder
    /// </summary>
    /// <param name="modifiedBy">Güncelleyen kullanıcının ID'si</param>
    public virtual void SetAuditInfo(string modifiedBy)
    {
        ModifiedBy = modifiedBy;
        SetModifiedDate();
    }

    /// <summary>
    /// Entity oluşturulurken audit bilgilerini set eder
    /// </summary>
    /// <param name="createdBy">Oluşturan kullanıcının ID'si</param>
    public virtual void SetCreationAuditInfo(string createdBy)
    {
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }
}
