using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
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
    /// İş ilanına göre başvuruları getirir
    /// </summary>
    public async Task<IEnumerable<CVApplication>> GetByJobPostingIdAsync(Guid jobPostingId)
    {
        return await _context.CVApplications
            .Where(cv => cv.JobPostingId == jobPostingId)
            .Include(cv => cv.User)
            .OrderByDescending(cv => cv.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının başvurularını getirir
    /// </summary>
    public async Task<IEnumerable<CVApplication>> GetByUserIdAsync(Guid userId)
    {
        return await _context.CVApplications
            .Where(cv => cv.UserId == userId)
            .Include(cv => cv.JobPosting)
            .ThenInclude(jp => jp.Company)
            .OrderByDescending(cv => cv.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Duruma göre başvuruları getirir
    /// </summary>
    public async Task<IEnumerable<CVApplication>> GetByStatusAsync(string status)
    {
        return await _context.CVApplications
            .Where(cv => cv.Status == status)
            .Include(cv => cv.User)
            .Include(cv => cv.JobPosting)
            .OrderByDescending(cv => cv.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının belirli bir ilana başvurusunu kontrol eder
    /// </summary>
    public async Task<bool> HasUserAppliedAsync(Guid userId, Guid jobPostingId)
    {
        return await _context.CVApplications
            .AnyAsync(cv => cv.UserId == userId && cv.JobPostingId == jobPostingId);
    }

    /// <summary>
    /// Başvuru sayısını getirir
    /// </summary>
    public async Task<int> GetApplicationCountAsync(Guid jobPostingId)
    {
        return await _context.CVApplications
            .CountAsync(cv => cv.JobPostingId == jobPostingId);
    }

    /// <summary>
    /// Tarih aralığına göre başvuruları getirir
    /// </summary>
    public async Task<IEnumerable<CVApplication>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.CVApplications
            .Where(cv => cv.CreatedAt >= startDate && cv.CreatedAt <= endDate)
            .Include(cv => cv.User)
            .Include(cv => cv.JobPosting)
            .OrderByDescending(cv => cv.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Şirket bazında başvuruları getirir
    /// </summary>
    public async Task<IEnumerable<CVApplication>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.CVApplications
            .Where(cv => cv.JobPosting.CompanyId == companyId)
            .Include(cv => cv.User)
            .Include(cv => cv.JobPosting)
            .OrderByDescending(cv => cv.CreatedAt)
            .ToListAsync();
    }
}
