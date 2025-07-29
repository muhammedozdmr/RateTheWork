using RateTheWork.Domain.Events;

namespace RateTheWork.Domain.Events.Warning;

/// <summary>
/// Uyarı iptal edildiğinde tetiklenen event
/// </summary>
public class WarningRevokedEvent : DomainEventBase
{
    public WarningRevokedEvent(
        string warningId,
        string userId,
        string adminUserId,
        string revokeReason,
        DateTime revokedAt)
    {
        WarningId = warningId;
        UserId = userId;
        AdminUserId = adminUserId;
        RevokeReason = revokeReason;
        RevokedAt = revokedAt;
    }

    public string WarningId { get; }
    public string UserId { get; }
    public string AdminUserId { get; }
    public string RevokeReason { get; }
    public DateTime RevokedAt { get; }
}