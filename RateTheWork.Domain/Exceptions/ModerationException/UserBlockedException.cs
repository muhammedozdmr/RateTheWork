namespace RateTheWork.Domain.Exceptions.ModerationException;

/// <summary>
/// Kullanıcı engellendi exception'ı
/// </summary>
public class UserBlockedException : DomainException
{
    public Guid UserId { get; }
    public string BlockReason { get; }
    public DateTime BlockedUntil { get; }
    public bool IsPermanent { get; }

    public UserBlockedException(Guid userId, string blockReason, DateTime blockedUntil, bool isPermanent = false)
        : base($"User is blocked. Reason: {blockReason}. {(isPermanent ? "Permanent block." : $"Blocked until: {blockedUntil:yyyy-MM-dd HH:mm}")}")
    {
        UserId = userId;
        BlockReason = blockReason;
        BlockedUntil = blockedUntil;
        IsPermanent = isPermanent;
    }
}
