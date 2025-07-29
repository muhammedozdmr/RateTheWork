using RateTheWork.Domain.Events;

namespace RateTheWork.Domain.Events.Warning;

/// <summary>
/// Kullanıcıya uyarı verildiğinde tetiklenen event
/// </summary>
public class WarningIssuedEvent : DomainEventBase
{
    public WarningIssuedEvent(
        string warningId,
        string userId,
        string adminUserId,
        string warningType,
        string reason,
        DateTime issuedAt)
    {
        WarningId = warningId;
        UserId = userId;
        AdminUserId = adminUserId;
        WarningType = warningType;
        Reason = reason;
        IssuedAt = issuedAt;
    }

    public string WarningId { get; }
    public string UserId { get; }
    public string AdminUserId { get; }
    public string WarningType { get; }
    public string Reason { get; }
    public DateTime IssuedAt { get; }
}