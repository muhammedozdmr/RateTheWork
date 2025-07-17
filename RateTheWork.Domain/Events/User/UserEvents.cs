namespace RateTheWork.Domain.Events.User;

/// <summary>
/// 1. Kullanıcı kayıt oldu event'i
/// </summary>
public record UserRegisteredEvent(
    string? UserId
    , string Email
    , string AnonymousUsername
    , string RegisterIp
    , string UserAgent
    , string? ReferrerUrl
    , string? RegistrationSource
    , DateTime RegisteredAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Email doğrulandı event'i
/// </summary>
public record UserEmailVerifiedEvent(
    string? UserId
    , string Email
    , string? PreviousEmail
    , string VerificationMethod
    , DateTime VerifiedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 3. Telefon doğrulandı event'i
/// </summary>
public record UserPhoneVerifiedEvent(
    string? UserId
    , string PhoneNumber
    , DateTime VerifiedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 4. TC Kimlik doğrulandı event'i
/// </summary>
public record UserTcIdentityVerifiedEvent(
    string? UserId
    , string DocumentUrl
    , DateTime VerifiedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 5. Kullanıcı profili güncellendi event'i
/// </summary>
public record UserProfileUpdatedEvent(
    string? UserId
    , string[] UpdatedFields
    , Dictionary<string, object>? OldValues
    , Dictionary<string, object>? NewValues
    , decimal ProfileCompleteness
    , DateTime UpdatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 6. Kullanıcı şifresi değiştirildi event'i
/// </summary>
public record UserPasswordChangedEvent(
    string? UserId
    , DateTime ChangedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 7. Kullanıcı hesabı silindi event'i
/// </summary>
public record UserAccountDeletedEvent(
    string? UserId
    , string Reason
    , int ReviewsCount
    , int BadgesCount
    , bool DataExported
    , string? FeedbackProvided
    , DateTime DeletedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 8. Sosyal medya hesabı ile giriş event'i
/// </summary>
public record UserRegisteredViaSocialEvent(
    string? UserId
    , string Provider
    , string ProviderId
    , DateTime RegisteredAt
    , DateTime OccurredOn = default) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 9. Kullanıcı giriş yaptı event'i
/// </summary>
public record UserLoginEvent(
    string? UserId
    , string LoginIp
    , string UserAgent
    , string? DeviceId
    , string LoginMethod
    , bool IsSuccessful
    , string? FailureReason
    , DateTime LoginAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 10. Kullanıcı çıkış yaptı event'i
/// </summary>
public record UserLogoutEvent(
    string? UserId
    , string LogoutReason
    , TimeSpan SessionDuration
    , DateTime LogoutAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 11. Kullanıcı oturumu sona erdi event'i
/// </summary>
public record UserSessionExpiredEvent(
    string? UserId
    , string SessionId
    , string ExpirationReason
    , DateTime ExpiredAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 12. Kullanıcı tercihleri güncellendi event'i
/// </summary>
public record UserPreferencesUpdatedEvent(
    string? UserId
    , Dictionary<string, object> OldPreferences
    , Dictionary<string, object> NewPreferences
    , string[] UpdatedCategories
    , DateTime UpdatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 13. Kullanıcı konumu güncellendi event'i
/// </summary>
public record UserLocationUpdatedEvent(
    string? UserId
    , string? OldCity
    , string NewCity
    , string? OldDistrict
    , string? NewDistrict
    , DateTime UpdatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 14. Kullanıcı deaktif edildi event'i
/// </summary>
public record UserDeactivatedEvent(
    string? UserId
    , string DeactivatedBy
    , string Reason
    , bool CanReactivate
    , DateTime? ReactivationDate
    , DateTime DeactivatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 15. Kullanıcı yeniden aktif edildi event'i
/// </summary>
public record UserReactivatedEvent(
    string? UserId
    , string ReactivatedBy
    , TimeSpan DeactivationDuration
    , DateTime ReactivatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
