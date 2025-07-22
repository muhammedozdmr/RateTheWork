using MediatR;
using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Enums.Subscription;

namespace RateTheWork.Application.Features.Subscriptions.Commands.CreateSubscription;

/// <summary>
/// Yeni üyelik oluşturma komutu
/// </summary>
public record CreateSubscriptionCommand : IRequest<Result<CreateSubscriptionResponse>>
{
    public string UserId { get; init; } = string.Empty;
    public SubscriptionType Type { get; init; }
    public BillingCycle BillingCycle { get; init; }
    public bool StartTrial { get; init; }
    public string? PaymentMethodId { get; init; }
}

/// <summary>
/// Üyelik oluşturma yanıtı
/// </summary>
public record CreateSubscriptionResponse
{
    public string SubscriptionId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public SubscriptionType Type { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public DateTime? NextBillingDate { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public List<FeatureType> Features { get; init; } = new();
}
