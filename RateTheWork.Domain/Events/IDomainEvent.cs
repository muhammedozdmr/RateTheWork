using MediatR;

namespace RateTheWork.Domain.Events;

/// <summary>
/// Domain event marker interface
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
