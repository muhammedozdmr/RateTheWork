using MediatR;

namespace RateTheWork.Domain.Events;

/// <summary>
/// Domain event marker interface
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Event'in gerçekleştiği zaman
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Event'in benzersiz ID'si
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Event version'u
    /// </summary>
    int Version { get; }
}
