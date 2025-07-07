namespace RateTheWork.Domain.Events;

public record UserRegisteredEvent(
    string? UserId,
    string Email,
    string AnonymousUsername,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
