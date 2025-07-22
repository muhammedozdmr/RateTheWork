namespace RateTheWork.Domain.Events;

/// <summary>
/// Tüm domain event'leri için base class
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    protected DomainEventBase()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        Version = 1;
    }

    public DateTime OccurredOn { get; }
    public Guid EventId { get; }
    public int Version { get; }
}
