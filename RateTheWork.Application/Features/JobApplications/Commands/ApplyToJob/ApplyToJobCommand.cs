using MediatR;
using RateTheWork.Application.Common.Models;

namespace RateTheWork.Application.Features.JobApplications.Commands.ApplyToJob;

/// <summary>
/// İş başvurusu yapma komutu
/// </summary>
public record ApplyToJobCommand : IRequest<Result<ApplyToJobResponse>>
{
    public string JobPostingId { get; init; } = string.Empty;
    public string ApplicantUserId { get; init; } = string.Empty;
    public string CoverLetter { get; init; } = string.Empty;
    public string? ResumeUrl { get; init; }
    public string? PortfolioUrl { get; init; }
    public string? LinkedInUrl { get; init; }
    public decimal? ExpectedSalary { get; init; }
    public int NoticePeriodDays { get; init; }
    public Dictionary<string, string>? Answers { get; init; } // Özel sorulara cevaplar
}

/// <summary>
/// İş başvurusu yapma yanıtı
/// </summary>
public record ApplyToJobResponse
{
    public string ApplicationId { get; init; } = string.Empty;
    public string JobPostingId { get; init; } = string.Empty;
    public string ApplicantUserId { get; init; } = string.Empty;
    public DateTime AppliedAt { get; init; }
    public string ApplicationNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime EstimatedResponseDate { get; init; }
}
