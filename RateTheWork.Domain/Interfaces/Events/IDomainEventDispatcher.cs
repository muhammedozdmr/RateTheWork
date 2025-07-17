using RateTheWork.Domain.Events;

namespace RateTheWork.Domain.Interfaces.Events;

/// <summary>
/// Domain event dispatcher interface'i
/// Event'leri ilgili handler'lara dağıtır
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Event'i yayınlar
    /// </summary>
    Task Dispatch<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    /// <summary>
    /// Birden fazla event'i yayınlar
    /// </summary>
    Task DispatchMultiple(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
