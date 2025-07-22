using RateTheWork.Application.Common.Specifications;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.JobPosting;

namespace RateTheWork.Application.Specifications;

/// <summary>
/// Active job postings specification
/// </summary>
public class ActiveJobPostingsSpec : BaseSpecification<JobPosting>
{
    public ActiveJobPostingsSpec()
    {
        AddCriteria(jp =>
            jp.Status == JobPostingStatus.Active &&
            jp.ExpiryDate > DateTime.UtcNow);

        AddInclude(jp => jp.Company);
        AddInclude(jp => jp.HRPersonnel);

        ApplyOrderByDescending(jp => jp.PublishDate);
    }
}

/// <summary>
/// Job postings by company specification
/// </summary>
public class JobPostingsByCompanySpec : BaseSpecification<JobPosting>
{
    public JobPostingsByCompanySpec(string companyId, bool activeOnly = true)
    {
        AddCriteria(jp => jp.CompanyId == companyId);

        if (activeOnly)
        {
            AddCriteria(jp =>
                jp.Status == JobPostingStatus.Active &&
                jp.ExpiryDate > DateTime.UtcNow);
        }

        AddInclude(jp => jp.HRPersonnel);
        ApplyOrderByDescending(jp => jp.CreatedAt);
    }
}

/// <summary>
/// Job postings by filter specification
/// </summary>
public class JobPostingsByFilterSpec : BaseSpecification<JobPosting>
{
    public JobPostingsByFilterSpec
    (
        string? searchTerm = null
        , string? city = null
        , JobType? jobType = null
        , WorkLocation? workLocation = null
        , ExperienceLevel? experienceLevel = null
        , decimal? minSalary = null
        , decimal? maxSalary = null
        , bool? isUrgent = null
        , int pageNumber = 1
        , int pageSize = 20
    )
    {
        // Base criteria - active postings
        AddCriteria(jp =>
            jp.Status == JobPostingStatus.Active &&
            jp.ExpiryDate > DateTime.UtcNow);

        // Search term
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            AddCriteria(jp =>
                jp.Title.ToLower().Contains(searchLower) ||
                jp.Description.ToLower().Contains(searchLower) ||
                jp.Company.Name.ToLower().Contains(searchLower));
        }

        // City filter
        if (!string.IsNullOrWhiteSpace(city))
        {
            AddCriteria(jp => jp.City == city);
        }

        // Job type filter
        if (jobType.HasValue)
        {
            AddCriteria(jp => jp.JobType == jobType.Value);
        }

        // Work location filter
        if (workLocation.HasValue)
        {
            AddCriteria(jp => jp.WorkLocation == workLocation.Value);
        }

        // Experience level filter
        if (experienceLevel.HasValue)
        {
            AddCriteria(jp => jp.ExperienceLevel == experienceLevel.Value);
        }

        // Salary range filter
        if (minSalary.HasValue)
        {
            AddCriteria(jp => jp.ShowSalary && jp.MaxSalary >= minSalary.Value);
        }

        if (maxSalary.HasValue)
        {
            AddCriteria(jp => jp.ShowSalary && jp.MinSalary <= maxSalary.Value);
        }

        // Urgent filter
        if (isUrgent.HasValue)
        {
            AddCriteria(jp => jp.IsUrgent == isUrgent.Value);
        }

        // Includes
        AddInclude(jp => jp.Company);
        AddInclude(jp => jp.HRPersonnel);

        // Ordering - urgent first, then by date
        ApplyOrderByDescending(jp => jp.IsUrgent);
        ApplyOrderByDescending(jp => jp.PublishDate);

        // Paging
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

/// <summary>
/// Suspicious job postings specification
/// </summary>
public class SuspiciousJobPostingsSpec : BaseSpecification<JobPosting>
{
    public SuspiciousJobPostingsSpec()
    {
        AddCriteria(jp =>
            // Çok yüksek hedef başvuru sayısı
            jp.TargetApplicationCount > 500 ||
            // Çok uzun işe alım süreci
            jp.EstimatedProcessDays > 90 ||
            // İlk mülakat tarihi çok geç
            (jp.FirstInterviewDate - jp.PublishDate).TotalDays > 30 ||
            // Aynı HR tarafından çok fazla ilan
            jp.HRPersonnel.PostedJobs > 50);

        AddInclude(jp => jp.Company);
        AddInclude(jp => jp.HRPersonnel);

        ApplyOrderByDescending(jp => jp.CreatedAt);
    }
}

/// <summary>
/// Job postings nearing expiry specification
/// </summary>
public class JobPostingsNearingExpirySpec : BaseSpecification<JobPosting>
{
    public JobPostingsNearingExpirySpec(int daysUntilExpiry = 7)
    {
        var expiryThreshold = DateTime.UtcNow.AddDays(daysUntilExpiry);

        AddCriteria(jp =>
            jp.Status == JobPostingStatus.Active &&
            jp.ExpiryDate <= expiryThreshold &&
            jp.ExpiryDate > DateTime.UtcNow);

        AddInclude(jp => jp.Company);
        AddInclude(jp => jp.HRPersonnel);

        ApplyOrderBy(jp => jp.ExpiryDate);
    }
}
