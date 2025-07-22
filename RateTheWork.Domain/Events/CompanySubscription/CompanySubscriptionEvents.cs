using RateTheWork.Domain.Enums.Subscription;

namespace RateTheWork.Domain.Events.CompanySubscription;

/// <summary>
/// Şirket üyeliği oluşturuldu eventi
/// </summary>
public class CompanySubscriptionCreatedEvent : DomainEventBase
{
    public CompanySubscriptionCreatedEvent
    (
        string subscriptionId
        , string companyId
        , SubscriptionType type
        , DateTime startDate
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        CompanyId = companyId;
        Type = type;
        StartDate = startDate;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string CompanyId { get; }
    public SubscriptionType Type { get; }
    public DateTime StartDate { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Şirket üyeliği yükseltildi eventi
/// </summary>
public class CompanySubscriptionUpgradedEvent : DomainEventBase
{
    public CompanySubscriptionUpgradedEvent
    (
        string subscriptionId
        , string companyId
        , SubscriptionType oldType
        , SubscriptionType newType
        , DateTime upgradedAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        CompanyId = companyId;
        OldType = oldType;
        NewType = newType;
        UpgradedAt = upgradedAt;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string CompanyId { get; }
    public SubscriptionType OldType { get; }
    public SubscriptionType NewType { get; }
    public DateTime UpgradedAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Şirket üyeliği iptal edildi eventi
/// </summary>
public class CompanySubscriptionCancelledEvent : DomainEventBase
{
    public CompanySubscriptionCancelledEvent
    (
        string subscriptionId
        , string companyId
        , SubscriptionType type
        , DateTime cancelledAt
        , string reason
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        CompanyId = companyId;
        Type = type;
        CancelledAt = cancelledAt;
        Reason = reason;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string CompanyId { get; }
    public SubscriptionType Type { get; }
    public DateTime CancelledAt { get; }
    public string Reason { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Şirket üyeliği yenilendi eventi
/// </summary>
public class CompanySubscriptionRenewedEvent : DomainEventBase
{
    public CompanySubscriptionRenewedEvent
    (
        string subscriptionId
        , string companyId
        , SubscriptionType type
        , DateTime nextBillingDate
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        CompanyId = companyId;
        Type = type;
        NextBillingDate = nextBillingDate;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string CompanyId { get; }
    public SubscriptionType Type { get; }
    public DateTime NextBillingDate { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İK personeli yetkilendirildi eventi
/// </summary>
public class HRPersonnelAuthorizedEvent : DomainEventBase
{
    public HRPersonnelAuthorizedEvent
    (
        string subscriptionId
        , string companyId
        , string personnelId
        , DateTime authorizedAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        CompanyId = companyId;
        PersonnelId = personnelId;
        AuthorizedAt = authorizedAt;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string CompanyId { get; }
    public string PersonnelId { get; }
    public DateTime AuthorizedAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İK personeli yetkisi kaldırıldı eventi
/// </summary>
public class HRPersonnelRevokedEvent : DomainEventBase
{
    public HRPersonnelRevokedEvent
    (
        string subscriptionId
        , string companyId
        , string personnelId
        , DateTime revokedAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        CompanyId = companyId;
        PersonnelId = personnelId;
        RevokedAt = revokedAt;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string CompanyId { get; }
    public string PersonnelId { get; }
    public DateTime RevokedAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İş ilanı kotası doldu eventi
/// </summary>
public class JobPostingQuotaExceededEvent : DomainEventBase
{
    public JobPostingQuotaExceededEvent
    (
        string subscriptionId
        , string companyId
        , int currentUsage
        , int limit
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        CompanyId = companyId;
        CurrentUsage = currentUsage;
        Limit = limit;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string CompanyId { get; }
    public int CurrentUsage { get; }
    public int Limit { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Yorum yanıtlama kotası doldu eventi
/// </summary>
public class ReviewResponseQuotaExceededEvent : DomainEventBase
{
    public ReviewResponseQuotaExceededEvent
    (
        string subscriptionId
        , string companyId
        , int currentUsage
        , int limit
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        CompanyId = companyId;
        CurrentUsage = currentUsage;
        Limit = limit;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string CompanyId { get; }
    public int CurrentUsage { get; }
    public int Limit { get; }
    public Dictionary<string, object>? Metadata { get; }
}
