namespace RateTheWork.Domain.Interfaces.Events;

/// <summary>
/// Event store interface'i - Event sourcing için
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Event'i saklar
    /// </summary>
    Task SaveEventAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;
    
    /// <summary>
    /// Aggregate'in event'lerini getirir
    /// </summary>
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Belirli bir tarihten sonraki event'leri getirir
    /// </summary>
    Task<IEnumerable<IDomainEvent>> GetEventsAfterAsync(DateTime after, CancellationToken cancellationToken = default);
}
