using RateTheWork.Domain.Events;

namespace RateTheWork.Domain.Common;

/// <summary>
/// DDD Aggregate Root base class.
/// Aggregate'lerin kök entity'leri bu sınıftan türer.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    /// <summary>
    /// Aggregate içindeki değişiklikleri takip eden versiyon numarası
    /// Optimistic concurrency control için kullanılır
    /// </summary>
    public int Version { get; protected set; }

    /// <summary>
    /// Yeni aggregate root oluşturur
    /// </summary>
    protected AggregateRoot() : base()
    {
        Version = 0;
    }

    /// <summary>
    /// Varolan bir aggregate root'u yüklerken kullanılır
    /// </summary>
    protected AggregateRoot(string? id, DateTime createdAt, DateTime? modifiedAt, int version) 
        : base(id, createdAt, modifiedAt)
    {
        Version = version;
    }

    /// <summary>
    /// Versiyon numarasını artırır
    /// </summary>
    protected void IncrementVersion()
    {
        Version++;
    }

    /// <summary>
    /// Domain event ekler ve versiyonu artırır
    /// </summary>
    protected void AddDomainEventWithVersioning(IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
        IncrementVersion();
    }
}
