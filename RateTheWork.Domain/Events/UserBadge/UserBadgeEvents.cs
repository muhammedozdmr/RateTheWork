namespace RateTheWork.Domain.Events.UserBadge;

/// <summary>
/// 1. Kullanıcıya rozet verildi event'i
/// </summary>
public record BadgeAwardedEvent(
    string UserBadgeId,
    string UserId,
    string BadgeId,
    DateTime AwardedAt,
    string? AwardReason,
    string? SpecialNote,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Kullanıcı rozeti görüntüledi event'i
/// </summary>
public record BadgeViewedEvent(
    string UserBadgeId,
    string UserId,
    string BadgeId,
    DateTime ViewedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 3. Kullanıcı rozeti gösterildi event'i
/// </summary>
public record BadgeDisplayedEvent(
    string UserBadgeId,
    string UserId,
    string BadgeId,
    DateTime DisplayedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 4. Kullanıcı rozeti gizlendi event'i
/// </summary>
public record BadgeHiddenEvent(
    string UserBadgeId,
    string UserId,
    string BadgeId,
    DateTime HiddenAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
