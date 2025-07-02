namespace RateTheWork.Domain.Entities.Common;

/// <summary>
/// Tüm entity'lerin türediği temel sınıf
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Entity'nin benzersiz kimliği (GUID)
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Entity'nin oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Entity'nin son güncellenme tarihi
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// BaseEntity constructor - Yeni entity oluşturulduğunda otomatik değerleri set eder
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
    }
}
