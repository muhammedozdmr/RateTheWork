using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Enums.JobApplication;

namespace RateTheWork.Application.Services.Interfaces;

/// <summary>
/// İş başvurusu yönetimi servisi
/// </summary>
public interface IJobApplicationService
{
    /// <summary>
    /// Kullanıcının daha önce başvurup başvurmadığını kontrol eder
    /// </summary>
    Task<bool> HasUserAppliedAsync(string userId, string jobPostingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Başvuru durumunu günceller
    /// </summary>
    Task<Result> UpdateApplicationStatusAsync
    (
        string applicationId
        , ApplicationStatus newStatus
        , string? notes = null
        , CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Toplu durum güncelleme
    /// </summary>
    Task<Result<BatchUpdateResult>> BatchUpdateStatusAsync
    (
        List<string> applicationIds
        , ApplicationStatus newStatus
        , string? notes = null
        , CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Başvuru istatistiklerini getirir
    /// </summary>
    Task<Result<ApplicationStatistics>> GetApplicationStatisticsAsync
        (string jobPostingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının başvuru geçmişini getirir
    /// </summary>
    Task<Result<UserApplicationHistory>> GetUserApplicationHistoryAsync
        (string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Başvuru zaman çizelgesi oluşturur
    /// </summary>
    Task<Result<ApplicationTimeline>> GetApplicationTimelineAsync
        (string applicationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mülakat planlaması yapar
    /// </summary>
    Task<Result> ScheduleInterviewAsync
    (
        string applicationId
        , DateTime interviewDate
        , string location
        , string? notes = null
        , CancellationToken cancellationToken = default
    );

    /// <summary>
    /// İş teklifi yapar
    /// </summary>
    Task<Result> MakeJobOfferAsync
    (
        string applicationId
        , decimal offeredSalary
        , DateTime offerExpiryDate
        , string? notes = null
        , CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Başvuru notunu günceller
    /// </summary>
    Task<Result> UpdateApplicationNotesAsync
        (string applicationId, string notes, CancellationToken cancellationToken = default);
}

/// <summary>
/// Toplu güncelleme sonucu
/// </summary>
public record BatchUpdateResult
{
    public int TotalCount { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<string> FailedApplicationIds { get; init; } = new();
    public List<string> Errors { get; init; } = new();
}

/// <summary>
/// Başvuru istatistikleri
/// </summary>
public record ApplicationStatistics
{
    public string JobPostingId { get; init; } = string.Empty;
    public int TotalApplications { get; init; }
    public Dictionary<ApplicationStatus, int> StatusBreakdown { get; init; } = new();
    public decimal AverageTimeToRespond { get; init; }
    public decimal ConversionRate { get; init; }
    public Dictionary<DateTime, int> DailyApplications { get; init; } = new();
    public TopApplicantStats TopApplicants { get; init; } = new();
}

/// <summary>
/// En iyi başvuran istatistikleri
/// </summary>
public record TopApplicantStats
{
    public int TotalQualified { get; init; }
    public int TotalInterviewed { get; init; }
    public int TotalOffered { get; init; }
    public decimal QualificationRate { get; init; }
}

/// <summary>
/// Kullanıcı başvuru geçmişi
/// </summary>
public record UserApplicationHistory
{
    public string UserId { get; init; } = string.Empty;
    public int TotalApplications { get; init; }
    public int ActiveApplications { get; init; }
    public int SuccessfulApplications { get; init; }
    public decimal SuccessRate { get; init; }
    public List<ApplicationSummary> RecentApplications { get; init; } = new();
    public Dictionary<string, int> ApplicationsByIndustry { get; init; } = new();
}

/// <summary>
/// Başvuru özeti
/// </summary>
public record ApplicationSummary
{
    public string ApplicationId { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public ApplicationStatus Status { get; init; }
    public DateTime AppliedDate { get; init; }
    public DateTime? LastUpdateDate { get; init; }
}

/// <summary>
/// Başvuru zaman çizelgesi
/// </summary>
public record ApplicationTimeline
{
    public string ApplicationId { get; init; } = string.Empty;
    public List<TimelineEvent> Events { get; init; } = new();
}

/// <summary>
/// Zaman çizelgesi olayı
/// </summary>
public record TimelineEvent
{
    public DateTime Date { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? PerformedBy { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}
