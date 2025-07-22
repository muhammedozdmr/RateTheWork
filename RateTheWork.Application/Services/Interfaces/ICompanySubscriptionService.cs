using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Enums.Subscription;

namespace RateTheWork.Application.Services.Interfaces;

/// <summary>
/// Şirket üyelik yönetimi servisi
/// </summary>
public interface ICompanySubscriptionService
{
    /// <summary>
    /// Şirketin aktif üyeliğini kontrol eder
    /// </summary>
    Task<bool> HasActiveSubscriptionAsync(string companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Şirketin ilan verme yetkisini kontrol eder
    /// </summary>
    Task<bool> CanPostJobAsync(string companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Şirketin yorumlara yanıt verme yetkisini kontrol eder
    /// </summary>
    Task<bool> CanReplyToReviewsAsync(string companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// İK personeli yetkilendirme kontrolü
    /// </summary>
    Task<Result<bool>> CanAuthorizeHRPersonnelAsync
        (string companyId, string personnelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// İlan kotası kullanımını günceller
    /// </summary>
    Task<Result> IncrementJobPostingCountAsync(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// İlan kotası kullanımını azaltır
    /// </summary>
    Task<Result> DecrementJobPostingCountAsync(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Şirket üyelik istatistikleri
    /// </summary>
    Task<Result<CompanySubscriptionStatistics>> GetSubscriptionStatisticsAsync
        (string companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Yükseltme seçeneklerini getirir
    /// </summary>
    Task<Result<List<UpgradeOption>>> GetUpgradeOptionsAsync
        (string companyId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Şirket üyelik istatistikleri
/// </summary>
public record CompanySubscriptionStatistics
{
    public string CompanyId { get; init; } = string.Empty;
    public SubscriptionType CurrentPlan { get; init; }
    public int TotalJobPostings { get; init; }
    public int ActiveJobPostings { get; init; }
    public int RemainingJobPostings { get; init; }
    public int TotalHRPersonnel { get; init; }
    public int ActiveHRPersonnel { get; init; }
    public int RemainingHRPersonnel { get; init; }
    public decimal MonthlySpend { get; init; }
    public decimal TotalSpend { get; init; }
    public DateTime MemberSince { get; init; }
    public DateTime? NextBillingDate { get; init; }
}

/// <summary>
/// Yükseltme seçeneği
/// </summary>
public record UpgradeOption
{
    public SubscriptionType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal MonthlyPrice { get; init; }
    public decimal YearlyPrice { get; init; }
    public int MaxJobPostings { get; init; }
    public int MaxHRPersonnel { get; init; }
    public List<FeatureType> Features { get; init; } = new();
    public decimal Savings { get; init; }
}
