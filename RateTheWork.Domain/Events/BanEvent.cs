using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Events;

/// <summary>
/// Kullanıcı banlandı event'i
/// </summary>
public record UserBannedEvent(
    string BanId,
    string UserId,
    string AdminId,
    string Reason,
    Ban.BanType BanType,
    DateTime? UnbanDate,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Ban kaldırıldı event'i
/// </summary>
public record BanLiftedEvent(
    string BanId,
    string UserId,
    string LiftedByAdminId,
    string Reason,
    Ban.BanType BanType,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Ban süresi uzatıldı event'i
/// </summary>
public record BanExtendedEvent(
    string BanId,
    string UserId,
    string ExtendedByAdminId,
    int AdditionalDays,
    string Reason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Ban kalıcı yapıldı event'i
/// </summary>
public record BanMadePermanentEvent(
    string BanId,
    string UserId,
    string AdminId,
    string Reason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
