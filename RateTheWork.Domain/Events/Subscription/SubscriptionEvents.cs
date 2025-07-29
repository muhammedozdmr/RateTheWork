using RateTheWork.Domain.Enums.Subscription;

namespace RateTheWork.Domain.Events.Subscription;

/// <summary>
/// Üyelik oluşturuldu eventi
/// </summary>
public class SubscriptionCreatedEvent : DomainEventBase
{
    public SubscriptionCreatedEvent
    (
        string subscriptionId
        , string userId
        , SubscriptionType type
        , DateTime startDate
        , DateTime? trialEndDate = null
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        Type = type;
        StartDate = startDate;
        TrialEndDate = trialEndDate;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string UserId { get; }
    public SubscriptionType Type { get; }
    public DateTime StartDate { get; }
    public DateTime? TrialEndDate { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Üyelik yükseltildi eventi
/// </summary>
public class SubscriptionUpgradedEvent : DomainEventBase
{
    public SubscriptionUpgradedEvent
    (
        string subscriptionId
        , string userId
        , SubscriptionType oldType
        , SubscriptionType newType
        , DateTime upgradedAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        OldType = oldType;
        NewType = newType;
        UpgradedAt = upgradedAt;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string UserId { get; }
    public SubscriptionType OldType { get; }
    public SubscriptionType NewType { get; }
    public DateTime UpgradedAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Üyelik iptal edildi eventi
/// </summary>
public class SubscriptionCancelledEvent : DomainEventBase
{
    public SubscriptionCancelledEvent
    (
        string subscriptionId
        , string userId
        , SubscriptionType type
        , DateTime cancelledAt
        , string reason
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        Type = type;
        CancelledAt = cancelledAt;
        Reason = reason;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string UserId { get; }
    public SubscriptionType Type { get; }
    public DateTime CancelledAt { get; }
    public string Reason { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Üyelik yenilendi eventi
/// </summary>
public class SubscriptionRenewedEvent : DomainEventBase
{
    public SubscriptionRenewedEvent
    (
        string subscriptionId
        , string userId
        , SubscriptionType type
        , DateTime nextBillingDate
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        Type = type;
        NextBillingDate = nextBillingDate;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string UserId { get; }
    public SubscriptionType Type { get; }
    public DateTime NextBillingDate { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Üyelik süresi doldu eventi
/// </summary>
public class SubscriptionExpiredEvent : DomainEventBase
{
    public SubscriptionExpiredEvent
    (
        string subscriptionId
        , string userId
        , SubscriptionType type
        , DateTime expiredAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        Type = type;
        ExpiredAt = expiredAt;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string UserId { get; }
    public SubscriptionType Type { get; }
    public DateTime ExpiredAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Deneme süresi başladı eventi
/// </summary>
public class TrialStartedEvent : DomainEventBase
{
    public TrialStartedEvent
    (
        string subscriptionId
        , string userId
        , SubscriptionType type
        , DateTime trialEndDate
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        Type = type;
        TrialEndDate = trialEndDate;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string UserId { get; }
    public SubscriptionType Type { get; }
    public DateTime TrialEndDate { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Deneme süresi bitti eventi
/// </summary>
public class TrialEndedEvent : DomainEventBase
{
    public TrialEndedEvent
    (
        string subscriptionId
        , string userId
        , SubscriptionType type
        , bool convertedToPaid
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        Type = type;
        ConvertedToPaid = convertedToPaid;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string UserId { get; }
    public SubscriptionType Type { get; }
    public bool ConvertedToPaid { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Kullanım limiti aşıldı eventi
/// </summary>
public class UsageLimitExceededEvent : DomainEventBase
{
    public UsageLimitExceededEvent
    (
        string subscriptionId
        , string userId
        , string feature
        , int currentUsage
        , int limit
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        Feature = feature;
        CurrentUsage = currentUsage;
        Limit = limit;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string UserId { get; }
    public string Feature { get; }
    public int CurrentUsage { get; }
    public int Limit { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Ödeme başarısız eventi
/// </summary>
public class PaymentFailedEvent : DomainEventBase
{
    public PaymentFailedEvent
    (
        string subscriptionId
        , string userId
        , decimal amount
        , string currency
        , string failureReason
        , int attemptNumber
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        Amount = amount;
        Currency = currency;
        FailureReason = failureReason;
        AttemptNumber = attemptNumber;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string UserId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string FailureReason { get; }
    public int AttemptNumber { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Ödeme başarılı eventi
/// </summary>
public class PaymentSucceededEvent : DomainEventBase
{
    public PaymentSucceededEvent
    (
        string subscriptionId
        , string userId
        , decimal amount
        , string currency
        , string paymentMethodId
        , string invoiceId
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        Amount = amount;
        Currency = currency;
        PaymentMethodId = paymentMethodId;
        InvoiceId = invoiceId;
        Metadata = metadata;
    }

    public string SubscriptionId { get; }
    public string UserId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string PaymentMethodId { get; }
    public string InvoiceId { get; }
    public Dictionary<string, object>? Metadata { get; }
}
