namespace RateTheWork.Domain.Events;

public record ReviewCreatedEvent(
    string ReviewId,
    string UserId,
    string CompanyId,
    decimal Rating,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

