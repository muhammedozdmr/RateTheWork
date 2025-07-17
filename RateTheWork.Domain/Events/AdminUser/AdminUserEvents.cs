namespace RateTheWork.Domain.Events.AdminUser;

/// <summary>
/// 1. Admin kullanıcı oluşturuldu event'i
/// </summary>
public record AdminUserCreatedEvent(
    string? AdminUserId
    , string Username
    , string Email
    , string Role
    , string CreatedBy
    , DateTime CreatedAt
) : DomainEventBase;

/// <summary>
/// 2. Admin giriş yaptı event'i
/// </summary>
public record AdminLoginEvent(
    string? AdminUserId
    , string IpAddress
    , string UserAgent
    , DateTime LoginAt
) : DomainEventBase;

/// <summary>
/// 3. Admin başarısız giriş denemesi event'i
/// </summary>
public record AdminFailedLoginEvent(
    string Username
    , string IpAddress
    , int FailedAttempts
    , DateTime AttemptedAt
) : DomainEventBase;

/// <summary>
/// 4. Admin hesabı kilitlendi event'i
/// </summary>
public record AdminAccountLockedEvent(
    string? AdminUserId
    , DateTime LockedUntil
    , string Reason
    , DateTime LockedAt
) : DomainEventBase;

/// <summary>
/// 5. Admin rolü değiştirildi event'i
/// </summary>
public record AdminRoleChangedEvent(
    string? AdminUserId
    , string OldRole
    , string NewRole
    , string ChangedBy
    , DateTime ChangedAt
) : DomainEventBase;
