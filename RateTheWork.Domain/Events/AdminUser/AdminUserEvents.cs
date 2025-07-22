namespace RateTheWork.Domain.Events.AdminUser;

/// <summary>
/// 1. Admin kullanıcı oluşturuldu event'i
/// </summary>
public class AdminUserCreatedEvent : DomainEventBase
{
    public AdminUserCreatedEvent
    (
        string? adminUserId
        , string username
        , string email
        , string role
        , string createdBy
        , DateTime createdAt
    ) : base()
    {
        AdminUserId = adminUserId;
        Username = username;
        Email = email;
        Role = role;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }

    public string? AdminUserId { get; }
    public string Username { get; }
    public string Email { get; }
    public string Role { get; }
    public string CreatedBy { get; }
    public DateTime CreatedAt { get; }
}

/// <summary>
/// 2. Admin giriş yaptı event'i
/// </summary>
public class AdminLoginEvent : DomainEventBase
{
    public AdminLoginEvent
    (
        string? adminUserId
        , string ipAddress
        , string userAgent
        , DateTime loginAt
    ) : base()
    {
        AdminUserId = adminUserId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        LoginAt = loginAt;
    }

    public string? AdminUserId { get; }
    public string IpAddress { get; }
    public string UserAgent { get; }
    public DateTime LoginAt { get; }
}

/// <summary>
/// 3. Admin başarısız giriş denemesi event'i
/// </summary>
public class AdminFailedLoginEvent : DomainEventBase
{
    public AdminFailedLoginEvent
    (
        string username
        , string ipAddress
        , int failedAttempts
        , DateTime attemptedAt
    ) : base()
    {
        Username = username;
        IpAddress = ipAddress;
        FailedAttempts = failedAttempts;
        AttemptedAt = attemptedAt;
    }

    public string Username { get; }
    public string IpAddress { get; }
    public int FailedAttempts { get; }
    public DateTime AttemptedAt { get; }
}

/// <summary>
/// 4. Admin hesabı kilitlendi event'i
/// </summary>
public class AdminAccountLockedEvent : DomainEventBase
{
    public AdminAccountLockedEvent
    (
        string? adminUserId
        , DateTime lockedUntil
        , string reason
        , DateTime lockedAt
    ) : base()
    {
        AdminUserId = adminUserId;
        LockedUntil = lockedUntil;
        Reason = reason;
        LockedAt = lockedAt;
    }

    public string? AdminUserId { get; }
    public DateTime LockedUntil { get; }
    public string Reason { get; }
    public DateTime LockedAt { get; }
}

/// <summary>
/// 5. Admin rolü değiştirildi event'i
/// </summary>
public class AdminRoleChangedEvent : DomainEventBase
{
    public AdminRoleChangedEvent
    (
        string? adminUserId
        , string oldRole
        , string newRole
        , string changedBy
        , DateTime changedAt
    ) : base()
    {
        AdminUserId = adminUserId;
        OldRole = oldRole;
        NewRole = newRole;
        ChangedBy = changedBy;
        ChangedAt = changedAt;
    }

    public string? AdminUserId { get; }
    public string OldRole { get; }
    public string NewRole { get; }
    public string ChangedBy { get; }
    public DateTime ChangedAt { get; }
}
