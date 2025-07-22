using MediatR;
using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Enums.Subscription;

namespace RateTheWork.Application.Features.Subscriptions.Commands.UpgradeSubscription;

/// <summary>
/// Üyelik yükseltme komutu
/// </summary>
public record UpgradeSubscriptionCommand : IRequest<Result<UpgradeSubscriptionResponse>>
{
    public string SubscriptionId { get; init; } = string.Empty;
    public SubscriptionType NewType { get; init; }
    public string? PaymentMethodId { get; init; }
}

/// <summary>
/// Üyelik yükseltme yanıtı
/// </summary>
public record UpgradeSubscriptionResponse
{
    public string SubscriptionId { get; init; } = string.Empty;
    public SubscriptionType OldType { get; init; }
    public SubscriptionType NewType { get; init; }
    public decimal NewPrice { get; init; }
    public DateTime UpgradedAt { get; init; }
    public DateTime NextBillingDate { get; init; }
    public List<FeatureType> NewFeatures { get; init; } = new();
}
