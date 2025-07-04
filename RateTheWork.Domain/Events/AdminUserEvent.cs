namespace RateTheWork.Domain.Events;

/// <summary>
/// Admin kullanıcı oluşturuldu event'i
/// </summary>
public record AdminUserCreatedEvent(
    string AdminUserId,
    string Username,
    string Email,
    string Role,
    string CreatedByAdminId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Admin başarılı giriş event'i
/// </summary>
public record AdminLoginSuccessEvent(
    string AdminUserId,
    string Username,
    DateTime LoginTime,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Admin başarısız giriş event'i
/// </summary>
public record AdminLoginFailedEvent(
    string AdminUserId,
    string Username,
    string IpAddress,
    int FailedAttemptCount,
    bool IsAccountLocked,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Admin hesabı kilitlendi event'i
/// </summary>
public record AdminAccountLockedEvent(
    string AdminUserId,
    string Username,
    DateTime LockedUntil,
    string Reason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Admin hesap kilidi açıldı event'i
/// </summary>
public record AdminAccountUnlockedEvent(
    string AdminUserId,
    string Username,
    string UnlockedByAdminId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Admin şifresi değişti event'i
/// </summary>
public record AdminPasswordChangedEvent(
    string AdminUserId,
    string Username,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Admin 2FA etkinleştirildi event'i
/// </summary>
public record AdminTwoFactorEnabledEvent(
    string AdminUserId,
    string Username,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Admin 2FA devre dışı bırakıldı event'i
/// </summary>
public record AdminTwoFactorDisabledEvent(
    string AdminUserId,
    string Username,
    string DisabledByAdminId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Admin rolü değişti event'i
/// </summary>
public record AdminRoleChangedEvent(
    string AdminUserId,
    string Username,
    string OldRole,
    string NewRole,
    string ChangedByAdminId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Admin hesabı deaktive edildi event'i
/// </summary>
public record AdminDeactivatedEvent(
    string AdminUserId,
    string Username,
    string DeactivatedByAdminId,
    string Reason,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Admin hesabı aktive edildi event'i
/// </summary>
public record AdminActivatedEvent(
    string AdminUserId,
    string Username,
    string ActivatedByAdminId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// Admin email değişti event'i
/// </summary>
public record AdminEmailChangedEvent(
    string AdminUserId,
    string Username,
    string OldEmail,
    string NewEmail,
    string ChangedByAdminId,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
