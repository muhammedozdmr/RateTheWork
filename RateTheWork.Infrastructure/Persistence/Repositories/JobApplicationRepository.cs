using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.JobApplication;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

public class JobApplicationRepository : BaseRepository<JobApplication>, IJobApplicationRepository
{
    public JobApplicationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<JobApplication>> GetByJobPostingIdAsync(string jobPostingId, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(ja => ja.JobPostingId == jobPostingId.ToString())
            .OrderByDescending(ja => ja.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<JobApplication>> GetByApplicantUserIdAsync
        (string applicantUserId, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(ja => ja.ApplicantUserId == applicantUserId)
            .OrderByDescending(ja => ja.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<JobApplication>> GetByStatusAsync(ApplicationStatus status, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(ja => ja.Status == status)
            .OrderByDescending(ja => ja.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> HasUserAppliedAsync(string jobPostingId, string applicantUserId)
    {
        return await _dbSet
            .AnyAsync(ja => ja.ApplicantUserId == applicantUserId && ja.JobPostingId == jobPostingId.ToString());
    }

    public async Task<List<JobApplication>> GetByHRPersonnelIdAsync
        (string hrPersonnelId, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .OrderByDescending(ja => ja.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<JobApplication>> GetByCompanyIdAsync(string companyId, int page = 1, int pageSize = 20)
    {
        if (!Guid.TryParse(companyId, out var companyGuid))
            return new List<JobApplication>();

        return await _dbSet
            .Where(ja => ja.JobPosting.CompanyId == companyGuid)
            .ThenInclude(jp => jp.Branch)
            .OrderByDescending(ja => ja.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<JobApplication>> GetByDateRangeAsync
        (DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(ja => ja.CreatedAt >= startDate && ja.CreatedAt <= endDate)
            .OrderByDescending(ja => ja.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // Additional helper methods
    public async Task<IEnumerable<JobApplication>> GetByJobPostingAsync
        (Guid jobPostingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ja => ja.JobPostingId == jobPostingId.ToString())
            .OrderByDescending(ja => ja.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JobApplication>> GetByUserAsync
        (Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ja => ja.ApplicantUserId == userId.ToString())
            .OrderByDescending(ja => ja.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<JobApplication?> GetByUserAndJobAsync
        (Guid userId, Guid jobPostingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                ja => ja.ApplicantUserId == userId.ToString() && ja.JobPostingId == jobPostingId.ToString()
                , cancellationToken);
    }

    public async Task<IEnumerable<JobApplication>> GetPendingApplicationsAsync
        (CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ja => ja.Status == ApplicationStatus.Received)
            .OrderBy(ja => ja.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetApplicationCountByStatusAsync
        (Guid jobPostingId, ApplicationStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(ja => ja.JobPostingId == jobPostingId.ToString() && ja.Status == status, cancellationToken);
    }

    public async Task<Dictionary<ApplicationStatus, int>> GetApplicationStatisticsAsync
        (Guid jobPostingId, CancellationToken cancellationToken = default)
    {
        var statistics = await _dbSet
            .Where(ja => ja.JobPostingId == jobPostingId.ToString())
            .GroupBy(ja => ja.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return statistics.ToDictionary(s => s.Status, s => s.Count);
    }

    public async Task<bool> HasUserAppliedAsync
        (Guid userId, Guid jobPostingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(ja => ja.ApplicantUserId == userId.ToString() && ja.JobPostingId == jobPostingId.ToString()
                , cancellationToken);
    }

    public async Task<JobApplication?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .ThenInclude(ca => ca.Review)
            .FirstOrDefaultAsync(ja => ja.Id == id.ToString(), cancellationToken);
    }

    public async Task<IEnumerable<JobApplication>> GetRecentApplicationsAsync
        (int days, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        return await _dbSet
            .Where(ja => ja.CreatedAt >= startDate)
            .OrderByDescending(ja => ja.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
