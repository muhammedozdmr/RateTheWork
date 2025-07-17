using MediatR;

namespace RateTheWork.Domain.Events;

/// <summary>
/// Tüm domain event'lerin base class'ı
/// </summary>
public abstract record DomainEventBase : INotification, IDomainEvent
{
    protected DomainEventBase()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        Version = 1;
    }

    /// <summary>
    /// Event'i oluşturan kullanıcı/sistem
    /// </summary>
    public string? CreatedBy { get; init; }

    /// <summary>
    /// Event metadata'sı
    /// </summary>
    public Dictionary<string, object?>? Metadata { get; init; }

    /// <summary>
    /// Event'in gerçekleştiği zaman
    /// </summary>
    public DateTime OccurredOn { get; init; }

    /// <summary>
    /// Event'in benzersiz ID'si
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// Event version'u (event sourcing için)
    /// </summary>
    public int Version { get; init; }
}
