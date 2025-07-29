using RateTheWork.Domain.Events;

namespace RateTheWork.Domain.Events.Ban;

/// <summary>
/// Kullanıcı yasağı kaldırıldığında tetiklenen event
/// </summary>
public class BanLiftedEvent : DomainEventBase
{
    public BanLiftedEvent(
        string banId,
        string userId,
        string adminUserId,
        string liftReason,
        DateTime liftedAt)
    {
        BanId = banId;
        UserId = userId;
        AdminUserId = adminUserId;
        LiftReason = liftReason;
        LiftedAt = liftedAt;
    }

    public string BanId { get; }
    public string UserId { get; }
    public string AdminUserId { get; }
    public string LiftReason { get; }
    public DateTime LiftedAt { get; }
}