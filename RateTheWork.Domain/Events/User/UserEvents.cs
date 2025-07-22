namespace RateTheWork.Domain.Events.User;

/// <summary>
/// 1. Kullanıcı kayıt oldu event'i
/// </summary>
public class UserRegisteredEvent : DomainEventBase
{
    public UserRegisteredEvent
    (
        string? userId
        , string email
        , string anonymousUsername
        , string registerIp
        , string userAgent
        , string? referrerUrl
        , string? registrationSource
        , DateTime registeredAt
    ) : base()
    {
        UserId = userId;
        Email = email;
        AnonymousUsername = anonymousUsername;
        RegisterIp = registerIp;
        UserAgent = userAgent;
        ReferrerUrl = referrerUrl;
        RegistrationSource = registrationSource;
        RegisteredAt = registeredAt;
    }

    public string? UserId { get; }
    public string Email { get; }
    public string AnonymousUsername { get; }
    public string RegisterIp { get; }
    public string UserAgent { get; }
    public string? ReferrerUrl { get; }
    public string? RegistrationSource { get; }
    public DateTime RegisteredAt { get; }
}

/// <summary>
/// 2. Email doğrulandı event'i
/// </summary>
public class UserEmailVerifiedEvent : DomainEventBase
{
    public UserEmailVerifiedEvent
    (
        string? userId
        , string email
        , string? previousEmail
        , string verificationMethod
        , DateTime verifiedAt
    ) : base()
    {
        UserId = userId;
        Email = email;
        PreviousEmail = previousEmail;
        VerificationMethod = verificationMethod;
        VerifiedAt = verifiedAt;
    }

    public string? UserId { get; }
    public string Email { get; }
    public string? PreviousEmail { get; }
    public string VerificationMethod { get; }
    public DateTime VerifiedAt { get; }
}

/// <summary>
/// 3. Telefon doğrulandı event'i
/// </summary>
public class UserPhoneVerifiedEvent : DomainEventBase
{
    public UserPhoneVerifiedEvent
    (
        string? userId
        , string phoneNumber
        , DateTime verifiedAt
    ) : base()
    {
        UserId = userId;
        PhoneNumber = phoneNumber;
        VerifiedAt = verifiedAt;
    }

    public string? UserId { get; }
    public string PhoneNumber { get; }
    public DateTime VerifiedAt { get; }
}

/// <summary>
/// 4. TC Kimlik doğrulandı event'i
/// </summary>
public class UserTcIdentityVerifiedEvent : DomainEventBase
{
    public UserTcIdentityVerifiedEvent
    (
        string? userId
        , string documentUrl
        , DateTime verifiedAt
    ) : base()
    {
        UserId = userId;
        DocumentUrl = documentUrl;
        VerifiedAt = verifiedAt;
    }

    public string? UserId { get; }
    public string DocumentUrl { get; }
    public DateTime VerifiedAt { get; }
}

/// <summary>
/// 5. Kullanıcı profili güncellendi event'i
/// </summary>
public class UserProfileUpdatedEvent : DomainEventBase
{
    public UserProfileUpdatedEvent
    (
        string? userId
        , string[] updatedFields
        , Dictionary<string, object>? oldValues
        , Dictionary<string, object>? newValues
        , decimal profileCompleteness
        , DateTime updatedAt
    ) : base()
    {
        UserId = userId;
        UpdatedFields = updatedFields;
        OldValues = oldValues;
        NewValues = newValues;
        ProfileCompleteness = profileCompleteness;
        UpdatedAt = updatedAt;
    }

    public string? UserId { get; }
    public string[] UpdatedFields { get; }
    public Dictionary<string, object>? OldValues { get; }
    public Dictionary<string, object>? NewValues { get; }
    public decimal ProfileCompleteness { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 6. Kullanıcı şifresi değiştirildi event'i
/// </summary>
public class UserPasswordChangedEvent : DomainEventBase
{
    public UserPasswordChangedEvent
    (
        string? userId
        , DateTime changedAt
    ) : base()
    {
        UserId = userId;
        ChangedAt = changedAt;
    }

    public string? UserId { get; }
    public DateTime ChangedAt { get; }
}

/// <summary>
/// 7. Kullanıcı hesabı silindi event'i
/// </summary>
public class UserAccountDeletedEvent : DomainEventBase
{
    public UserAccountDeletedEvent
    (
        string? userId
        , string reason
        , int reviewsCount
        , int badgesCount
        , bool dataExported
        , string? feedbackProvided
        , DateTime deletedAt
    ) : base()
    {
        UserId = userId;
        Reason = reason;
        ReviewsCount = reviewsCount;
        BadgesCount = badgesCount;
        DataExported = dataExported;
        FeedbackProvided = feedbackProvided;
        DeletedAt = deletedAt;
    }

    public string? UserId { get; }
    public string Reason { get; }
    public int ReviewsCount { get; }
    public int BadgesCount { get; }
    public bool DataExported { get; }
    public string? FeedbackProvided { get; }
    public DateTime DeletedAt { get; }
}

/// <summary>
/// 8. Sosyal medya hesabı ile giriş event'i
/// </summary>
public class UserRegisteredViaSocialEvent : DomainEventBase
{
    public UserRegisteredViaSocialEvent
    (
        string? userId
        , string provider
        , string providerId
        , DateTime registeredAt
    ) : base()
    {
        UserId = userId;
        Provider = provider;
        ProviderId = providerId;
        RegisteredAt = registeredAt;
    }

    public string? UserId { get; }
    public string Provider { get; }
    public string ProviderId { get; }
    public DateTime RegisteredAt { get; }
}

/// <summary>
/// 9. Kullanıcı giriş yaptı event'i
/// </summary>
public class UserLoginEvent : DomainEventBase
{
    public UserLoginEvent
    (
        string? userId
        , string loginIp
        , string userAgent
        , string? deviceId
        , string loginMethod
        , bool isSuccessful
        , string? failureReason
        , DateTime loginAt
    ) : base()
    {
        UserId = userId;
        LoginIp = loginIp;
        UserAgent = userAgent;
        DeviceId = deviceId;
        LoginMethod = loginMethod;
        IsSuccessful = isSuccessful;
        FailureReason = failureReason;
        LoginAt = loginAt;
    }

    public string? UserId { get; }
    public string LoginIp { get; }
    public string UserAgent { get; }
    public string? DeviceId { get; }
    public string LoginMethod { get; }
    public bool IsSuccessful { get; }
    public string? FailureReason { get; }
    public DateTime LoginAt { get; }
}

/// <summary>
/// 10. Kullanıcı çıkış yaptı event'i
/// </summary>
public class UserLogoutEvent : DomainEventBase
{
    public UserLogoutEvent
    (
        string? userId
        , string logoutReason
        , TimeSpan sessionDuration
        , DateTime logoutAt
    ) : base()
    {
        UserId = userId;
        LogoutReason = logoutReason;
        SessionDuration = sessionDuration;
        LogoutAt = logoutAt;
    }

    public string? UserId { get; }
    public string LogoutReason { get; }
    public TimeSpan SessionDuration { get; }
    public DateTime LogoutAt { get; }
}

/// <summary>
/// 11. Kullanıcı oturumu sona erdi event'i
/// </summary>
public class UserSessionExpiredEvent : DomainEventBase
{
    public UserSessionExpiredEvent
    (
        string? userId
        , string sessionId
        , string expirationReason
        , DateTime expiredAt
    ) : base()
    {
        UserId = userId;
        SessionId = sessionId;
        ExpirationReason = expirationReason;
        ExpiredAt = expiredAt;
    }

    public string? UserId { get; }
    public string SessionId { get; }
    public string ExpirationReason { get; }
    public DateTime ExpiredAt { get; }
}

/// <summary>
/// 12. Kullanıcı tercihleri güncellendi event'i
/// </summary>
public class UserPreferencesUpdatedEvent : DomainEventBase
{
    public UserPreferencesUpdatedEvent
    (
        string? userId
        , Dictionary<string, object> oldPreferences
        , Dictionary<string, object> newPreferences
        , string[] updatedCategories
        , DateTime updatedAt
    ) : base()
    {
        UserId = userId;
        OldPreferences = oldPreferences;
        NewPreferences = newPreferences;
        UpdatedCategories = updatedCategories;
        UpdatedAt = updatedAt;
    }

    public string? UserId { get; }
    public Dictionary<string, object> OldPreferences { get; }
    public Dictionary<string, object> NewPreferences { get; }
    public string[] UpdatedCategories { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 13. Kullanıcı konumu güncellendi event'i
/// </summary>
public class UserLocationUpdatedEvent : DomainEventBase
{
    public UserLocationUpdatedEvent
    (
        string? userId
        , string? oldCity
        , string newCity
        , string? oldDistrict
        , string? newDistrict
        , DateTime updatedAt
    ) : base()
    {
        UserId = userId;
        OldCity = oldCity;
        NewCity = newCity;
        OldDistrict = oldDistrict;
        NewDistrict = newDistrict;
        UpdatedAt = updatedAt;
    }

    public string? UserId { get; }
    public string? OldCity { get; }
    public string NewCity { get; }
    public string? OldDistrict { get; }
    public string? NewDistrict { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 14. Kullanıcı deaktif edildi event'i
/// </summary>
public class UserDeactivatedEvent : DomainEventBase
{
    public UserDeactivatedEvent
    (
        string? userId
        , string deactivatedBy
        , string reason
        , bool canReactivate
        , DateTime? reactivationDate
        , DateTime deactivatedAt
    ) : base()
    {
        UserId = userId;
        DeactivatedBy = deactivatedBy;
        Reason = reason;
        CanReactivate = canReactivate;
        ReactivationDate = reactivationDate;
        DeactivatedAt = deactivatedAt;
    }

    public string? UserId { get; }
    public string DeactivatedBy { get; }
    public string Reason { get; }
    public bool CanReactivate { get; }
    public DateTime? ReactivationDate { get; }
    public DateTime DeactivatedAt { get; }
}

/// <summary>
/// 15. Kullanıcı yeniden aktif edildi event'i
/// </summary>
public class UserReactivatedEvent : DomainEventBase
{
    public UserReactivatedEvent
    (
        string? userId
        , string reactivatedBy
        , TimeSpan deactivationDuration
        , DateTime reactivatedAt
    ) : base()
    {
        UserId = userId;
        ReactivatedBy = reactivatedBy;
        DeactivationDuration = deactivationDuration;
        ReactivatedAt = reactivatedAt;
    }

    public string? UserId { get; }
    public string ReactivatedBy { get; }
    public TimeSpan DeactivationDuration { get; }
    public DateTime ReactivatedAt { get; }
}
