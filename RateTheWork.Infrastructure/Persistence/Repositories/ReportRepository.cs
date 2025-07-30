using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Report;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

/// <summary>
/// Şikayet işlemleri için repository implementasyonu
/// </summary>
public class ReportRepository : BaseRepository<Report>, IReportRepository
{
    public ReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Queryable interface sağlar
    /// </summary>
    public IQueryable<Report> GetQueryable()
    {
        return _context.Reports.AsQueryable();
    }

    /// <summary>
    /// Belirli bir entity'ye yapılan şikayetleri getirir
    /// </summary>
    public async Task<List<Report>> GetReportsByEntityAsync(string entityType, string entityId)
    {
        return await _context.Reports
            .Where(r => r.EntityType == entityType && r.EntityId == entityId)
            .OrderByDescending(r => r.ReportedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının yaptığı şikayetleri getirir
    /// </summary>
    public async Task<List<Report>> GetReportsByUserAsync(string userId)
    {
        return await _context.Reports
            .Where(r => r.ReporterUserId == userId)
            .OrderByDescending(r => r.ReportedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Bekleyen şikayetleri getirir
    /// </summary>
    public async Task<List<Report>> GetPendingReportsAsync()
    {
        return await _context.Reports
            .Where(r => r.Status == ReportStatus.Pending)
            .OrderBy(r => r.ReportedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Belirli durumdaki şikayetleri getirir
    /// </summary>
    public async Task<List<Report>> GetReportsByStatusAsync(ReportStatus status)
    {
        return await _context.Reports
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.ReportedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının belirli bir entity'ye daha önce şikayet yapıp yapmadığını kontrol eder
    /// </summary>
    public async Task<bool> HasUserReportedEntityAsync(string userId, string entityType, string entityId)
    {
        return await _context.Reports
            .AnyAsync(r => r.ReporterUserId == userId &&
                           r.EntityType == entityType &&
                           r.EntityId == entityId);
    }

    /// <summary>
    /// Belirli bir tarih aralığındaki şikayetleri getirir
    /// </summary>
    public async Task<List<Report>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Reports
            .Where(r => r.ReportedAt >= startDate && r.ReportedAt <= endDate)
            .OrderByDescending(r => r.ReportedAt)
            .ToListAsync();
    }

    /// <summary>
    /// En çok şikayet edilen entity'leri getirir
    /// </summary>
    public async Task<Dictionary<string, int>> GetMostReportedEntitiesAsync(string entityType, int topCount = 10)
    {
        return await _context.Reports
            .Where(r => r.EntityType == entityType)
            .GroupBy(r => r.EntityId)
            .Select(g => new { EntityId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(topCount)
            .ToDictionaryAsync(x => x.EntityId, x => x.Count);
    }
}
