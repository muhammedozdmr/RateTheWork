using RateTheWork.Domain.Events;

namespace RateTheWork.Domain.Interfaces.Events;

/// <summary>
/// Domain event handler interface'i
/// </summary>
/// <typeparam name="TEvent">İşlenecek event tipi</typeparam>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Event'i işler
    /// </summary>
    Task Handle(TEvent domainEvent, CancellationToken cancellationToken = default);
}
