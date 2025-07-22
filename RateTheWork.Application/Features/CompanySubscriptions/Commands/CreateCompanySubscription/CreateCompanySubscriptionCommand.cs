using MediatR;
using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Enums.Subscription;

namespace RateTheWork.Application.Features.CompanySubscriptions.Commands.CreateCompanySubscription;

/// <summary>
/// Şirket üyeliği oluşturma komutu
/// </summary>
public record CreateCompanySubscriptionCommand : IRequest<Result<CreateCompanySubscriptionResponse>>
{
    public string CompanyId { get; init; } = string.Empty;
    public SubscriptionType Type { get; init; }
    public BillingCycle BillingCycle { get; init; }
    public int MaxJobPostings { get; init; }
    public int MaxHRPersonnel { get; init; }
    public string? PaymentMethodId { get; init; }
    public string? BillingEmail { get; init; }
    public string? TaxNumber { get; init; }
}

/// <summary>
/// Şirket üyeliği oluşturma yanıtı
/// </summary>
public record CreateCompanySubscriptionResponse
{
    public string SubscriptionId { get; init; } = string.Empty;
    public string CompanyId { get; init; } = string.Empty;
    public SubscriptionType Type { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime NextBillingDate { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int MaxJobPostings { get; init; }
    public int MaxHRPersonnel { get; init; }
    public List<FeatureType> Features { get; init; } = new();
}
