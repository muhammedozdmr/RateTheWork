using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.CVApplication;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

/// <summary>
/// CV başvuruları için repository implementasyonu
/// </summary>
public class CVApplicationRepository : BaseRepository<CVApplication>, ICVApplicationRepository
{
    public CVApplicationRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Kullanıcının başvurularını getirir
    /// </summary>
    public async Task<List<CVApplication>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20)
    {
        return await _context.CVApplications
            .Where(cv => cv.UserId == userId)
            .Include(cv => cv.Company)
            .OrderByDescending(cv => cv.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Duruma göre başvuruları getirir
    /// </summary>
    public async Task<List<CVApplication>> GetByStatusAsync(CVApplicationStatus status, int page = 1, int pageSize = 20)
    {
        return await _context.CVApplications
            .Where(cv => cv.Status == status)
            .Include(cv => cv.User)
            .Include(cv => cv.Company)
            .OrderByDescending(cv => cv.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Şirket bazında başvuruları getirir
    /// </summary>
    public async Task<List<CVApplication>> GetByCompanyIdAsync(string companyId, int page = 1, int pageSize = 20)
    {
        return await _context.CVApplications
            .Where(cv => cv.CompanyId == companyId)
            .Include(cv => cv.User)
            .Include(cv => cv.Company)
            .OrderByDescending(cv => cv.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Departmana göre başvuruları getirir
    /// </summary>
    public async Task<List<CVApplication>> GetByDepartmentIdAsync(string departmentId, int page = 1, int pageSize = 20)
    {
        return await _context.CVApplications
            .Where(cv => cv.DepartmentIds.Contains(departmentId))
            .Include(cv => cv.User)
            .Include(cv => cv.Departments)
            .OrderByDescending(cv => cv.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Süresi dolmuş başvuruları getirir
    /// </summary>
    public async Task<List<CVApplication>> GetExpiredApplicationsAsync(DateTime asOfDate)
    {
        return await _context.CVApplications
            .Where(cv => cv.ExpiryDate < asOfDate &&
                         cv.Status == CVApplicationStatus.Pending)
            .Include(cv => cv.User)
            .Include(cv => cv.Company)
            .OrderBy(cv => cv.ExpiryDate)
            .ToListAsync();
    }

    /// <summary>
    /// Geri bildirim süresi dolmuş başvuruları getirir
    /// </summary>
    public async Task<List<CVApplication>> GetFeedbackOverdueApplicationsAsync(DateTime asOfDate)
    {
        return await _context.CVApplications
            .Where(cv => cv.FeedbackDeadline.HasValue && cv.FeedbackDeadline.Value < asOfDate &&
                         (cv.Status == CVApplicationStatus.Pending || cv.Status == CVApplicationStatus.Viewed))
            .Include(cv => cv.User)
            .Include(cv => cv.Company)
            .OrderBy(cv => cv.FeedbackDeadline)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının şirkete daha önce başvuru yapıp yapmadığını kontrol eder
    /// </summary>
    public async Task<bool> HasUserAppliedToCompanyAsync(string userId, string companyId, DateTime withinDays)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-withinDays.Day);
        return await _context.CVApplications
            .AnyAsync(cv => cv.UserId == userId &&
                            cv.CompanyId == companyId &&
                            cv.CreatedAt >= cutoffDate);
    }

    /// <summary>
    /// İstatistikleri getirir
    /// </summary>
    public async Task<CVApplicationStatistics> GetStatisticsAsync(string companyId)
    {
        var applications = await _context.CVApplications
            .Where(cv => cv.CompanyId == companyId)
            .Include(cv => cv.Departments)
            .ToListAsync();

        var statistics = new CVApplicationStatistics
        {
            TotalApplications = applications.Count
            , PendingApplications = applications.Count(a => a.Status == CVApplicationStatus.Pending)
            , ViewedApplications = applications.Count(a => a.Status == CVApplicationStatus.Viewed)
            , DownloadedApplications = applications.Count(a => a.Status == CVApplicationStatus.Downloaded)
            , RespondedApplications = applications.Count(a => a.Status == CVApplicationStatus.Accepted ||
                                                              a.Status == CVApplicationStatus.Rejected ||
                                                              a.Status == CVApplicationStatus.OnHold)
        };

        // Ortalama yanıt süresi (gün cinsinden)
        var respondedApps = applications.Where(a => a.RespondedAt.HasValue).ToList();
        if (respondedApps.Any())
        {
            statistics.AverageResponseTime = (decimal)respondedApps
                .Average(a => (a.RespondedAt!.Value - a.CreatedAt).TotalDays);
        }

        // Departman bazında başvurular
        statistics.ApplicationsByDepartment = applications
            .Where(a => a.Departments != null && a.Departments.Any())
            .SelectMany(a => a.Departments.Select(d => new { a, d }))
            .GroupBy(x => x.d.Name)
            .ToDictionary(g => g.Key, g => g.Count());

        // Durum bazında başvurular
        statistics.ApplicationsByStatus = applications
            .GroupBy(a => a.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        return statistics;
    }

    /// <summary>
    /// İş ilanına göre başvuruları getirir
    /// </summary>
    public async Task<IEnumerable<CVApplication>> GetByJobPostingIdAsync(Guid jobPostingId)
    {
        return await _context.CVApplications
            .Where(cv => cv.CompanyId == jobPostingId.ToString())
            .Include(cv => cv.User)
            .OrderByDescending(cv => cv.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının belirli bir ilana başvurusunu kontrol eder
    /// </summary>
    public async Task<bool> HasUserAppliedAsync(string userId, Guid jobPostingId)
    {
        return await _context.CVApplications
            .AnyAsync(cv => cv.UserId == userId && cv.CompanyId == jobPostingId.ToString());
    }

    /// <summary>
    /// Başvuru sayısını getirir
    /// </summary>
    public async Task<int> GetApplicationCountAsync(Guid jobPostingId)
    {
        return await _context.CVApplications
            .CountAsync(cv => cv.CompanyId == jobPostingId.ToString());
    }

    /// <summary>
    /// Tarih aralığına göre başvuruları getirir
    /// </summary>
    public async Task<IEnumerable<CVApplication>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.CVApplications
            .Where(cv => cv.CreatedAt >= startDate && cv.CreatedAt <= endDate)
            .Include(cv => cv.User)
            .Include(cv => cv.Company)
            .OrderByDescending(cv => cv.CreatedAt)
            .ToListAsync();
    }
}
