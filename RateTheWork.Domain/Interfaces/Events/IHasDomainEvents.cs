namespace RateTheWork.Domain.Interfaces.Events;

/// <summary>
/// Domain event'leri olan entity interface'i
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Entity'nin domain event'leri
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    
    /// <summary>
    /// Yeni domain event ekler
    /// </summary>
    void AddDomainEvent(IDomainEvent domainEvent);
    
    /// <summary>
    /// Domain event'leri temizler
    /// </summary>
    void ClearDomainEvents();
}

