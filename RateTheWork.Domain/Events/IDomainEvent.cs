using MediatR;

namespace RateTheWork.Domain.Events;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
