namespace RateTheWork.Domain.Exceptions.SubscriptionException;

/// <summary>
/// Subscription işlemleri için genel exception
/// </summary>
public class SubscriptionException : DomainException
{
    public SubscriptionException(string message)
        : base(message)
    {
        Severity = ExceptionSeverity.Medium;
    }

    public SubscriptionException(string message, Exception innerException)
        : base(message, innerException)
    {
        Severity = ExceptionSeverity.Medium;
    }

    public static SubscriptionException InvalidSubscriptionType(string userId, string type)
    {
        return new SubscriptionException($"Invalid subscription type '{type}' for user {userId}")
            .WithContext("UserId", userId)
            .WithContext("Type", type) as SubscriptionException;
    }

    public static SubscriptionException TrialAlreadyUsed(string userId)
    {
        return new SubscriptionException($"User {userId} has already used their trial period")
            .WithContext("UserId", userId)
            .WithUserMessageKey("subscription.trial_already_used") as SubscriptionException;
    }

    public static SubscriptionException AlreadyActive(string userId)
    {
        return new SubscriptionException($"User {userId} already has an active subscription")
            .WithContext("UserId", userId)
            .WithUserMessageKey("subscription.already_active") as SubscriptionException;
    }

    public static SubscriptionException CannotCancel(string subscriptionId, string reason)
    {
        return new SubscriptionException($"Cannot cancel subscription {subscriptionId}: {reason}")
            .WithContext("SubscriptionId", subscriptionId)
            .WithContext("Reason", reason) as SubscriptionException;
    }

    public static SubscriptionException InvalidTransition
        (string subscriptionId, string currentStatus, string targetStatus)
    {
        return new SubscriptionException(
                $"Cannot transition subscription {subscriptionId} from {currentStatus} to {targetStatus}")
            .WithContext("SubscriptionId", subscriptionId)
            .WithContext("CurrentStatus", currentStatus)
            .WithContext("TargetStatus", targetStatus) as SubscriptionException;
    }

    public static SubscriptionException PaymentFailed(string subscriptionId, string reason)
    {
        return new SubscriptionException($"Payment failed for subscription {subscriptionId}: {reason}")
            .WithContext("SubscriptionId", subscriptionId)
            .WithContext("Reason", reason)
            .WithSeverity(ExceptionSeverity.High) as SubscriptionException;
    }

    public static SubscriptionException SubscriptionExpired(string subscriptionId)
    {
        return new SubscriptionException($"Subscription {subscriptionId} has expired")
            .WithContext("SubscriptionId", subscriptionId)
            .WithUserMessageKey("subscription.expired") as SubscriptionException;
    }

    public static SubscriptionException QuotaExceeded(string subscriptionId, string feature, int limit)
    {
        return new SubscriptionException(
                $"Quota exceeded for feature '{feature}' in subscription {subscriptionId}. Limit: {limit}")
            .WithContext("SubscriptionId", subscriptionId)
            .WithContext("Feature", feature)
            .WithContext("Limit", limit)
            .WithUserMessageKey("subscription.quota_exceeded") as SubscriptionException;
    }
}
