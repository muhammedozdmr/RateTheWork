using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Models;
using RateTheWork.Application.Services.Interfaces;
using RateTheWork.Domain.Enums.Subscription;

namespace RateTheWork.Application.Services.Implementations;

/// <summary>
/// Üyelik yönetimi servisi implementasyonu
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public SubscriptionService(IApplicationDbContext context, IDateTimeService dateTimeService)
    {
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public Task<bool> HasUsedTrialAsync(string userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasActiveSubscriptionAsync(string userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasFeatureAsync(string userId, FeatureType feature, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> CheckUsageLimitAsync
        (string subscriptionId, string feature, int requestedUsage = 1, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> UpdateUsageAsync
        (string subscriptionId, string feature, int usage, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> ProcessExpiredSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> ProcessExpiredTrialsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> SendRenewalRemindersAsync
        (int daysBeforeExpiry = 7, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<decimal>> CalculateSubscriptionPriceAsync
    (
        SubscriptionType type
        , BillingCycle billingCycle
        , string? promoCode = null
        , CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<Result<PromoCodeValidationResult>> ValidatePromoCodeAsync
        (string promoCode, SubscriptionType type, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
