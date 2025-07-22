using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Services.Interfaces;

/// <summary>
/// İK personeli yönetimi servisi
/// </summary>
public interface IHRPersonnelService
{
    /// <summary>
    /// İK personelinin doğrulama durumunu kontrol eder
    /// </summary>
    Task<bool> IsVerifiedAsync(string personnelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// İK personelinin aktif durumunu kontrol eder
    /// </summary>
    Task<bool> IsActiveAsync(string personnelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// İK personelinin şirket eşleşmesini kontrol eder
    /// </summary>
    Task<Result<bool>> ValidateCompanyMatchAsync
        (string personnelId, string companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// İK personeli performans metriklerini günceller
    /// </summary>
    Task<Result> UpdatePerformanceMetricsAsync
        (string personnelId, HRPerformanceUpdate update, CancellationToken cancellationToken = default);

    /// <summary>
    /// Güven skorunu günceller
    /// </summary>
    Task<Result> UpdateTrustScoreAsync
        (string personnelId, decimal scoreChange, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// İK personeli performans raporunu getirir
    /// </summary>
    Task<Result<HRPerformanceReport>> GetPerformanceReportAsync
        (string personnelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Şirketin İK personellerini listeler
    /// </summary>
    Task<Result<List<HRPersonnel>>> GetCompanyHRPersonnelAsync
        (string companyId, bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// İK personeli doğrulama işlemi
    /// </summary>
    Task<Result> VerifyHRPersonnelAsync
        (string personnelId, string verifiedBy, CancellationToken cancellationToken = default);
}

/// <summary>
/// İK performans güncelleme modeli
/// </summary>
public record HRPerformanceUpdate
{
    public int? PostedJobs { get; init; }
    public int? HiredCandidates { get; init; }
    public decimal? AverageHiringTime { get; init; }
    public decimal? ResponseRate { get; init; }
}

/// <summary>
/// İK performans raporu
/// </summary>
public record HRPerformanceReport
{
    public string PersonnelId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public decimal TrustScore { get; init; }
    public HRPerformanceMetrics Metrics { get; init; } = new();
    public List<HRActivity> RecentActivities { get; init; } = new();
    public Dictionary<string, decimal> ScoreBreakdown { get; init; } = new();
    public List<string> Strengths { get; init; } = new();
    public List<string> AreasForImprovement { get; init; } = new();
}

/// <summary>
/// İK performans metrikleri
/// </summary>
public record HRPerformanceMetrics
{
    public int TotalJobPostings { get; init; }
    public int ActiveJobPostings { get; init; }
    public int TotalApplicationsReceived { get; init; }
    public int TotalHires { get; init; }
    public decimal HiringRate { get; init; }
    public decimal AverageTimeToHire { get; init; }
    public decimal ApplicationResponseRate { get; init; }
    public decimal CandidateSatisfactionScore { get; init; }
}

/// <summary>
/// İK aktivitesi
/// </summary>
public record HRActivity
{
    public DateTime Date { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? RelatedJobPostingId { get; init; }
}
