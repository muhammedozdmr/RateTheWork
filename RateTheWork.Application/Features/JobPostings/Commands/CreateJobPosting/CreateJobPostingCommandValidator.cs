using FluentValidation;

namespace RateTheWork.Application.Features.JobPostings.Commands.CreateJobPosting;

/// <summary>
/// İş ilanı oluşturma validasyonu
/// </summary>
public class CreateJobPostingCommandValidator : AbstractValidator<CreateJobPostingCommand>
{
    public CreateJobPostingCommandValidator()
    {
        // Temel bilgiler
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required");

        RuleFor(x => x.HRPersonnelId)
            .NotEmpty().WithMessage("HR Personnel ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Job title is required")
            .Length(3, 200).WithMessage("Title must be between 3 and 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Job description is required")
            .Length(50, 5000).WithMessage("Description must be between 50 and 5000 characters");

        RuleFor(x => x.JobType)
            .IsInEnum().WithMessage("Invalid job type");

        RuleFor(x => x.WorkLocation)
            .IsInEnum().WithMessage("Invalid work location");

        RuleFor(x => x.ExperienceLevel)
            .IsInEnum().WithMessage("Invalid experience level");

        // Lokasyon
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required");

        // Maaş bilgileri
        RuleFor(x => x.MinSalary)
            .GreaterThan(0).When(x => x.MinSalary.HasValue)
            .WithMessage("Minimum salary must be greater than 0");

        RuleFor(x => x.MaxSalary)
            .GreaterThan(x => x.MinSalary).When(x => x.MinSalary.HasValue && x.MaxSalary.HasValue)
            .WithMessage("Maximum salary must be greater than minimum salary");

        // Şeffaflık bilgileri (Zorunlu)
        RuleFor(x => x.FirstInterviewDate)
            .GreaterThan(DateTime.Now.AddDays(2))
            .WithMessage("First interview date must be at least 3 days from now");

        RuleFor(x => x.TargetApplicationCount)
            .InclusiveBetween(1, 1000)
            .WithMessage("Target application count must be between 1 and 1000");

        RuleFor(x => x.HiringProcess)
            .NotEmpty().WithMessage("Hiring process description is required")
            .Length(50, 2000).WithMessage("Hiring process must be between 50 and 2000 characters");

        RuleFor(x => x.EstimatedProcessDays)
            .InclusiveBetween(1, 180)
            .WithMessage("Estimated process days must be between 1 and 180");

        // Gereksinimler
        RuleFor(x => x.RequiredSkills)
            .NotEmpty().WithMessage("At least one required skill must be specified")
            .Must(x => x.Count <= 20).WithMessage("Maximum 20 required skills allowed");

        // Yayın ayarları
        RuleFor(x => x.PublishDate)
            .GreaterThanOrEqualTo(DateTime.Now.Date)
            .WithMessage("Publish date cannot be in the past");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.PublishDate)
            .WithMessage("Expiry date must be after publish date")
            .LessThanOrEqualTo(x => x.PublishDate.AddDays(90))
            .WithMessage("Job posting cannot be active for more than 90 days");

        // İlk mülakat tarihi kontrolü
        RuleFor(x => x.FirstInterviewDate)
            .GreaterThan(x => x.PublishDate)
            .WithMessage("First interview date must be after publish date")
            .LessThanOrEqualTo(x => x.ExpiryDate.AddDays(30))
            .WithMessage("First interview date cannot be more than 30 days after expiry date");
    }
}
