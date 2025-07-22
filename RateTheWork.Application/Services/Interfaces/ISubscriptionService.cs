using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Enums.Subscription;

namespace RateTheWork.Application.Services.Interfaces;

/// <summary>
/// Üyelik yönetimi servisi
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Kullanıcının deneme süresi hakkını kontrol eder
    /// </summary>
    Task<bool> HasUsedTrialAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının aktif üyeliğini kontrol eder
    /// </summary>
    Task<bool> HasActiveSubscriptionAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Üyelik özelliklerini kontrol eder
    /// </summary>
    Task<bool> HasFeatureAsync(string userId, FeatureType feature, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanım limitini kontrol eder
    /// </summary>
    Task<Result<bool>> CheckUsageLimitAsync
        (string subscriptionId, string feature, int requestedUsage = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanımı günceller
    /// </summary>
    Task<Result> UpdateUsageAsync
        (string subscriptionId, string feature, int usage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Süresi dolan üyelikleri işler
    /// </summary>
    Task<Result> ProcessExpiredSubscriptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deneme süresi biten üyelikleri işler
    /// </summary>
    Task<Result> ProcessExpiredTrialsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Yenileme hatırlatması gönderir
    /// </summary>
    Task<Result> SendRenewalRemindersAsync(int daysBeforeExpiry = 7, CancellationToken cancellationToken = default);

    /// <summary>
    /// Üyelik fiyatını hesaplar
    /// </summary>
    Task<Result<decimal>> CalculateSubscriptionPriceAsync
    (
        SubscriptionType type
        , BillingCycle billingCycle
        , string? promoCode = null
        , CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Promosyon kodu doğrular
    /// </summary>
    Task<Result<PromoCodeValidationResult>> ValidatePromoCodeAsync
        (string promoCode, SubscriptionType type, CancellationToken cancellationToken = default);
}

/// <summary>
/// Promosyon kodu doğrulama sonucu
/// </summary>
public record PromoCodeValidationResult
{
    public bool IsValid { get; init; }
    public decimal DiscountPercentage { get; init; }
    public decimal DiscountAmount { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? Message { get; init; }
}
