using MediatR;
using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Enums.Subscription;

namespace RateTheWork.Application.Features.Subscriptions.Queries.GetUserSubscription;

/// <summary>
/// Kullanıcı üyelik bilgisi sorgulama
/// </summary>
public record GetUserSubscriptionQuery : IRequest<Result<SubscriptionDto>>
{
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Üyelik bilgi DTO'su
/// </summary>
public record SubscriptionDto
{
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public SubscriptionType Type { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DateTime? NextBillingDate { get; init; }
    public BillingCycle BillingCycle { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsCancelled { get; init; }
    public DateTime? CancelledAt { get; init; }
    public bool IsTrialPeriod { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public List<FeatureType> Features { get; init; } = new();
    public Dictionary<string, int> UsageLimits { get; init; } = new();
    public Dictionary<string, int> CurrentUsage { get; init; } = new();
    public SubscriptionStatusDto Status { get; init; } = new();
}

public record SubscriptionStatusDto
{
    public bool IsExpired { get; init; }
    public bool IsInTrial { get; init; }
    public bool IsGracePeriod { get; init; }
    public int DaysUntilExpiry { get; init; }
    public int DaysUntilRenewal { get; init; }
}
