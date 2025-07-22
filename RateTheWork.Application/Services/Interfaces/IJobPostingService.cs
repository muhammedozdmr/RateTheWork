using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Services.Interfaces;

/// <summary>
/// İş ilanı yönetimi servisi
/// </summary>
public interface IJobPostingService
{
    /// <summary>
    /// Şüpheli havuz ilanı kontrolü yapar
    /// </summary>
    Task<Result<SuspiciousJobCheckResult>> CheckForSuspiciousPoolJobAsync
        (JobPosting jobPosting, CancellationToken cancellationToken = default);

    /// <summary>
    /// İK personelinin ilan verme yetkisini kontrol eder
    /// </summary>
    Task<Result<bool>> CanHRPersonnelPostJobAsync
        (string personnelId, string companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Şirketin ilan kotasını kontrol eder
    /// </summary>
    Task<Result<JobPostingQuotaResult>> CheckCompanyJobPostingQuotaAsync
        (string companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// İlan görüntülenme sayısını artırır
    /// </summary>
    Task<Result> IncrementViewCountAsync
        (string jobPostingId, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// İlan istatistiklerini getirir
    /// </summary>
    Task<Result<JobPostingStatistics>> GetJobPostingStatisticsAsync
        (string jobPostingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Benzer ilanları getirir
    /// </summary>
    Task<Result<List<JobPosting>>> GetSimilarJobPostingsAsync
        (string jobPostingId, int count = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// İlanın hedef başvuru sayısına ulaşıp ulaşmadığını kontrol eder
    /// </summary>
    Task<Result<bool>> HasReachedTargetApplicationsAsync
        (string jobPostingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Süresi dolan ilanları işler
    /// </summary>
    Task<Result> ProcessExpiredJobPostingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// İlan performans raporunu oluşturur
    /// </summary>
    Task<Result<JobPostingPerformanceReport>> GeneratePerformanceReportAsync
        (string jobPostingId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Şüpheli ilan kontrol sonucu
/// </summary>
public record SuspiciousJobCheckResult
{
    public bool IsSuspicious { get; init; }
    public List<string> Reasons { get; init; } = new();
    public decimal SuspicionScore { get; init; }
    public List<string> Recommendations { get; init; } = new();
}

/// <summary>
/// İlan kotası sonucu
/// </summary>
public record JobPostingQuotaResult
{
    public bool CanPost { get; init; }
    public int CurrentCount { get; init; }
    public int MaxAllowed { get; init; }
    public int Remaining { get; init; }
    public DateTime? QuotaResetDate { get; init; }
}

/// <summary>
/// İlan istatistikleri
/// </summary>
public record JobPostingStatistics
{
    public int TotalViews { get; init; }
    public int UniqueViews { get; init; }
    public int TotalApplications { get; init; }
    public int ShortlistedApplications { get; init; }
    public int InterviewedApplications { get; init; }
    public decimal ConversionRate { get; init; }
    public Dictionary<DateTime, int> DailyViews { get; init; } = new();
    public Dictionary<DateTime, int> DailyApplications { get; init; } = new();
}

/// <summary>
/// İlan performans raporu
/// </summary>
public record JobPostingPerformanceReport
{
    public string JobPostingId { get; init; } = string.Empty;
    public JobPostingStatistics Statistics { get; init; } = new();
    public decimal QualityScore { get; init; }
    public List<string> Strengths { get; init; } = new();
    public List<string> Improvements { get; init; } = new();
    public Dictionary<string, object> Benchmarks { get; init; } = new();
}
