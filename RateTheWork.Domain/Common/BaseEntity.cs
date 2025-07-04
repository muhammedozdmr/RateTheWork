using RateTheWork.Domain.Events;

namespace RateTheWork.Domain.Common;

/// <summary>
/// Tüm entity'lerin türediği temel sınıf.
/// Domain Events desteği içerir.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Entity'nin benzersiz kimliği (GUID)
    /// </summary>
    public string Id { get; protected set; }
    
    /// <summary>
    /// Entity'nin oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; protected set; }
    
    /// <summary>
    /// Entity'nin son güncellenme tarihi
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Domain event'leri (readonly)
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// BaseEntity constructor - Yeni entity oluşturulduğunda otomatik değerleri set eder
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = null;
    }

    /// <summary>
    /// Varolan bir entity'yi yüklerken kullanılır (örn: veritabanından)
    /// </summary>
    protected BaseEntity(string id, DateTime createdAt, DateTime? modifiedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        ModifiedAt = modifiedAt;
    }

    /// <summary>
    /// Domain event ekler
    /// </summary>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Belirli bir domain event'i kaldırır
    /// </summary>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Tüm domain event'leri temizler
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Entity güncelleme zamanını set eder
    /// </summary>
    protected void SetModifiedDate()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// İki entity'nin eşit olup olmadığını kontrol eder (ID bazlı)
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Entity'nin hash code'unu döner
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// == operatörü overload
    /// </summary>
    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (left is null && right is null)
            return true;
        
        if (left is null || right is null)
            return false;
        
        return left.Equals(right);
    }

    /// <summary>
    /// != operatörü overload
    /// </summary>
    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !(left == right);
    }
}
