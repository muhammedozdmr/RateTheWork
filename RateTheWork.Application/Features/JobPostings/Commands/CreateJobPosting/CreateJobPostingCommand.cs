using MediatR;
using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Enums.JobPosting;

namespace RateTheWork.Application.Features.JobPostings.Commands.CreateJobPosting;

/// <summary>
/// İş ilanı oluşturma komutu
/// </summary>
public record CreateJobPostingCommand : IRequest<Result<CreateJobPostingResponse>>
{
    // Temel bilgiler
    public string CompanyId { get; init; } = string.Empty;
    public string HRPersonnelId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public JobType JobType { get; init; }
    public WorkLocation WorkLocation { get; init; }
    public ExperienceLevel ExperienceLevel { get; init; }

    // Lokasyon bilgileri
    public string City { get; init; } = string.Empty;
    public string? District { get; init; }
    public string? Address { get; init; }

    // Maaş bilgileri
    public decimal? MinSalary { get; init; }
    public decimal? MaxSalary { get; init; }
    public string Currency { get; init; } = "TRY";
    public bool ShowSalary { get; init; }

    // Şeffaflık bilgileri (Zorunlu)
    public DateTime FirstInterviewDate { get; init; }
    public int TargetApplicationCount { get; init; }
    public string HiringProcess { get; init; } = string.Empty;
    public int EstimatedProcessDays { get; init; }
    public string? RejectionFeedbackCommitment { get; init; }

    // Gereksinimler
    public List<string> RequiredSkills { get; init; } = new();
    public List<string> PreferredSkills { get; init; } = new();
    public string? EducationLevel { get; init; }
    public List<string> Languages { get; init; } = new();

    // Yan haklar ve özellikler
    public List<string> Benefits { get; init; } = new();
    public Dictionary<string, object>? AdditionalInfo { get; init; }

    // Yayın ayarları
    public DateTime PublishDate { get; init; }
    public DateTime ExpiryDate { get; init; }
    public bool IsUrgent { get; init; }
}

/// <summary>
/// İş ilanı oluşturma yanıtı
/// </summary>
public record CreateJobPostingResponse
{
    public string JobPostingId { get; init; } = string.Empty;
    public string CompanyId { get; init; } = string.Empty;
    public string HRPersonnelId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public JobPostingStatus Status { get; init; }
    public DateTime PublishDate { get; init; }
    public DateTime ExpiryDate { get; init; }
    public DateTime FirstInterviewDate { get; init; }
    public int TargetApplicationCount { get; init; }
    public string ViewUrl { get; init; } = string.Empty;
}
