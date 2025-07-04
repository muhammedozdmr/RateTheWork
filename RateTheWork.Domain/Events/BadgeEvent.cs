namespace RateTheWork.Domain.Events;

/// <summary>
/// Rozet deaktive edildi event'i
/// </summary>
public record BadgeDeactivatedEvent(
    string BadgeId,
    string BadgeName,
    string Reason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Rozet aktive edildi event'i
/// </summary>
public record BadgeActivatedEvent(
    string BadgeId,
    string BadgeName,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Kullanıcıya rozet verildi event'i
/// </summary>
public record BadgeAwardedEvent(
    string UserBadgeId,
    string UserId,
    string BadgeId,
    DateTime AwardedAt,
    string? AwardReason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Kullanıcı rozeti görüntüledi event'i
/// </summary>
public record BadgeViewedEvent(
    string UserBadgeId,
    string UserId,
    string BadgeId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Kullanıcı rozeti gösterdi event'i
/// </summary>
public record BadgeDisplayedEvent(
    string UserBadgeId,
    string UserId,
    string BadgeId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Kullanıcı rozeti gizledi event'i
/// </summary>
public record BadgeHiddenEvent(
    string UserBadgeId,
    string UserId,
    string BadgeId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}