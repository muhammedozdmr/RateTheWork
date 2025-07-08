namespace RateTheWork.Domain.Events.User;

/// <summary>
/// 1. Kullanıcı kayıt oldu event'i
/// </summary>
public record UserRegisteredEvent(
    string UserId,
    string Email,
    string AnonymousUsername,
    DateTime RegisteredAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Email doğrulandı event'i
/// </summary>
public record UserEmailVerifiedEvent(
    string UserId,
    string Email,
    DateTime VerifiedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 3. Telefon doğrulandı event'i
/// </summary>
public record UserPhoneVerifiedEvent(
    string UserId,
    string PhoneNumber,
    DateTime VerifiedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 4. TC Kimlik doğrulandı event'i
/// </summary>
public record UserTcIdentityVerifiedEvent(
    string UserId,
    string DocumentUrl,
    DateTime VerifiedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 5. Kullanıcı profili güncellendi event'i
/// </summary>
public record UserProfileUpdatedEvent(
    string UserId,
    string[] UpdatedFields,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 6. Kullanıcı şifresi değiştirildi event'i
/// </summary>
public record UserPasswordChangedEvent(
    string UserId,
    DateTime ChangedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 7. Kullanıcı hesabı silindi event'i
/// </summary>
public record UserAccountDeletedEvent(
    string UserId,
    string Reason,
    DateTime DeletedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
