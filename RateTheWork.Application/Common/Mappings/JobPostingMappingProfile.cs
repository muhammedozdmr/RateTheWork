using AutoMapper;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.JobPosting;

namespace RateTheWork.Application.Common.Mappings;

/// <summary>
/// JobPosting entity mapping profili
/// </summary>
public class JobPostingMappingProfile : Profile
{
    public JobPostingMappingProfile()
    {
        // JobPosting -> JobPostingDto
        CreateMap<JobPosting, JobPostingDto>()
            .ForMember(dest => dest.DaysUntilExpiry, opt => opt.MapFrom(src =>
                (int)(src.ExpiryDate - DateTime.UtcNow).TotalDays))
            .ForMember(dest => dest.DaysUntilFirstInterview, opt => opt.MapFrom(src =>
                (int)(src.FirstInterviewDate - DateTime.UtcNow).TotalDays))
            .ForMember(dest => dest.ApplicationProgress, opt => opt.MapFrom(src =>
                src.TargetApplicationCount > 0 ? (decimal)src.ApplicationCount / src.TargetApplicationCount * 100 : 0));

        // JobPosting -> JobPostingDetailDto
        CreateMap<JobPosting, JobPostingDetailDto>()
            .IncludeBase<JobPosting, JobPostingDto>();

        // JobApplication -> JobApplicationDto
        CreateMap<JobApplication, JobApplicationDto>();

        // HRPersonnel -> HRPersonnelDto
        CreateMap<HRPersonnel, HRPersonnelDto>()
            .ForMember(dest => dest.PerformanceMetrics, opt => opt.MapFrom(src => new HRPerformanceMetricsDto
            {
                PostedJobs = src.PostedJobs, HiredCandidates = src.HiredCandidates
                , AverageHiringTime = src.AverageHiringTime, ResponseRate = src.ResponseRate
                , TrustScore = src.TrustScore
            }));
    }
}

/// <summary>
/// İş ilanı DTO'su
/// </summary>
public record JobPostingDto
{
    public string Id { get; init; } = string.Empty;
    public string CompanyId { get; init; } = string.Empty;
    public string HRPersonnelId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public JobType JobType { get; init; }
    public WorkLocation WorkLocation { get; init; }
    public ExperienceLevel ExperienceLevel { get; init; }
    public string City { get; init; } = string.Empty;
    public decimal? MinSalary { get; init; }
    public decimal? MaxSalary { get; init; }
    public bool ShowSalary { get; init; }
    public JobPostingStatus Status { get; init; }
    public DateTime PublishDate { get; init; }
    public DateTime ExpiryDate { get; init; }
    public DateTime FirstInterviewDate { get; init; }
    public int TargetApplicationCount { get; init; }
    public int ApplicationCount { get; init; }
    public int ViewCount { get; init; }
    public bool IsUrgent { get; init; }
    public int DaysUntilExpiry { get; init; }
    public int DaysUntilFirstInterview { get; init; }
    public decimal ApplicationProgress { get; init; }
}

/// <summary>
/// İş ilanı detay DTO'su
/// </summary>
public record JobPostingDetailDto : JobPostingDto
{
    public string? District { get; init; }
    public string? Address { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string HiringProcess { get; init; } = string.Empty;
    public int EstimatedProcessDays { get; init; }
    public string? RejectionFeedbackCommitment { get; init; }
    public List<string> RequiredSkills { get; init; } = new();
    public List<string> PreferredSkills { get; init; } = new();
    public string? EducationLevel { get; init; }
    public List<string> Languages { get; init; } = new();
    public List<string> Benefits { get; init; } = new();
    public Dictionary<string, object>? AdditionalInfo { get; init; }
    public HRPersonnelDto? HRPersonnel { get; init; }
}

/// <summary>
/// İş başvurusu DTO'su
/// </summary>
public record JobApplicationDto
{
    public string Id { get; init; } = string.Empty;
    public string JobPostingId { get; init; } = string.Empty;
    public string ApplicantUserId { get; init; } = string.Empty;
    public string ApplicantName { get; init; } = string.Empty;
    public string ApplicantEmail { get; init; } = string.Empty;
    public Domain.Enums.JobApplication.JobApplicationStatus Status { get; init; }
    public DateTime AppliedAt { get; init; }
    public string? CoverLetter { get; init; }
    public string? ResumeUrl { get; init; }
    public decimal? ExpectedSalary { get; init; }
    public int NoticePeriodDays { get; init; }
}

/// <summary>
/// İK personeli DTO'su
/// </summary>
public record HRPersonnelDto
{
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string CompanyId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public bool IsActive { get; init; }
    public string? LinkedInProfile { get; init; }
    public string? ProfilePhotoUrl { get; init; }
    public HRPerformanceMetricsDto PerformanceMetrics { get; init; } = new();
}

/// <summary>
/// İK performans metrikleri DTO'su
/// </summary>
public record HRPerformanceMetricsDto
{
    public int PostedJobs { get; init; }
    public int HiredCandidates { get; init; }
    public decimal AverageHiringTime { get; init; }
    public decimal ResponseRate { get; init; }
    public decimal TrustScore { get; init; }
}
