using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.JobPosting;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

public class JobPostingRepository : BaseRepository<JobPosting>, IJobPostingRepository
{
    public JobPostingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<JobPosting>> GetByCompanyIdAsync(string companyId, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(jp => jp.CompanyId == companyId.ToString())
            .OrderByDescending(jp => jp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<JobPosting>> GetByHRPersonnelIdAsync(string hrPersonnelId, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(jp => jp.HRPersonnelId == hrPersonnelId)
            .OrderByDescending(jp => jp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<JobPosting>> GetActiveJobPostingsAsync(int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(jp => jp.Status == JobPostingStatus.Active && jp.ApplicationDeadline > DateTime.UtcNow)
            .OrderByDescending(jp => jp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<JobPosting>> GetByStatusAsync(JobPostingStatus status, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(jp => jp.Status == status)
            .OrderByDescending(jp => jp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<JobPosting>> GetByCityAsync(string city, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(jp => jp.Location.Contains(city) && jp.Status == JobPostingStatus.Active)
            .OrderByDescending(jp => jp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<JobPosting>> GetByJobTypeAsync(JobType jobType, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(jp => jp.JobType == jobType && jp.Status == JobPostingStatus.Active)
            .OrderByDescending(jp => jp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<JobPosting>> GetUrgentJobPostingsAsync(int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(jp => jp.Status == JobPostingStatus.Active && jp.ApplicationDeadline > DateTime.UtcNow)
            .OrderByDescending(jp => jp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task IncrementViewCountAsync(string jobPostingId)
    {
        var jobPosting = await _dbSet.FindAsync(jobPostingId);
        if (jobPosting != null)
        {
            // ViewCount is readonly, need to use a method if available
            // For now, we'll skip this functionality
            await _context.SaveChangesAsync();
        }
    }

    // Additional helper methods
    public async Task<JobPosting?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // JobPosting doesn't have Slug property, find by title instead
        return await _dbSet
            .FirstOrDefaultAsync(jp => jp.Title.ToLower().Replace(" ", "-") == slug.ToLower(), cancellationToken);
    }

    public async Task<IEnumerable<JobPosting>> GetActiveJobsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(jp => jp.Status == JobPostingStatus.Active && jp.ApplicationDeadline > DateTime.UtcNow)
            .OrderByDescending(jp => jp.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JobPosting>> GetByCompanyAsync
        (Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(jp => jp.CompanyId == companyId.ToString())
            .OrderByDescending(jp => jp.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JobPosting>> GetByBranchAsync
        (Guid branchId, CancellationToken cancellationToken = default)
    {
        // JobPosting doesn't have BranchId, this would need to be handled differently
        return new List<JobPosting>();
    }

    public async Task<IEnumerable<JobPosting>> SearchJobsAsync
    (
        string? keyword
        , string? location
        , JobType? jobType
        , decimal? minSalary
        , decimal? maxSalary
        , CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet
            .Where(jp => jp.Status == JobPostingStatus.Active && jp.ApplicationDeadline > DateTime.UtcNow)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(jp =>
                jp.Title.Contains(keyword) ||
                jp.Description.Contains(keyword) ||
                jp.Company.Name.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(jp =>
                jp.Location.Contains(location));
        }

        if (jobType.HasValue)
        {
            query = query.Where(jp => jp.JobType == jobType.Value);
        }

        if (!string.IsNullOrEmpty(location))
        {
            query = query.Where(jp => jp.Location.Contains(location));
        }

        if (minSalary.HasValue)
        {
            query = query.Where(jp => jp.MinSalary >= minSalary.Value);
        }

        if (maxSalary.HasValue)
        {
            query = query.Where(jp => jp.MaxSalary <= maxSalary.Value);
        }

        return await query
            .OrderByDescending(jp => jp.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSlugUniqueAsync
        (string slug, Guid? excludeJobId = null, CancellationToken cancellationToken = default)
    {
        // JobPosting doesn't have Slug property, check by title instead
        var normalizedSlug = slug.ToLowerInvariant().Replace("-", " ");
        var query = _dbSet.Where(jp => jp.Title.ToLower().Replace(" ", "-") == slug.ToLower());

        if (excludeJobId.HasValue)
        {
            query = query.Where(jp => jp.Id != excludeJobId.Value.ToString());
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<int> GetApplicationCountAsync(Guid jobPostingId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<JobApplication>()
            .CountAsync(ja => ja.JobPostingId == jobPostingId.ToString(), cancellationToken);
    }

    public async Task<IEnumerable<JobPosting>> GetExpiringJobsAsync
        (int daysBeforeExpiry, CancellationToken cancellationToken = default)
    {
        var expiryDate = DateTime.UtcNow.AddDays(daysBeforeExpiry);

        return await _dbSet
            .Where(jp => jp.Status == JobPostingStatus.Active &&
                         jp.ApplicationDeadline <= expiryDate &&
                         jp.ApplicationDeadline > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<JobPosting?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(jp => jp.Id == id.ToString(), cancellationToken);
    }
}
