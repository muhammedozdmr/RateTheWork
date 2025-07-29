using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

/// <summary>
/// Raporlama işlemleri için repository implementasyonu
/// </summary>
public class ReportRepository : BaseRepository<Report>, IReportRepository
{
    public ReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Rapor tipine göre raporları getirir
    /// </summary>
    public async Task<IEnumerable<Report>> GetByTypeAsync(string reportType)
    {
        return await _context.Reports
            .Where(r => r.Type == reportType)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının oluşturduğu raporları getirir
    /// </summary>
    public async Task<IEnumerable<Report>> GetByCreatedByAsync(Guid userId)
    {
        return await _context.Reports
            .Where(r => r.CreatedById == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Tarih aralığına göre raporları getirir
    /// </summary>
    public async Task<IEnumerable<Report>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Reports
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Durum bazında raporları getirir
    /// </summary>
    public async Task<IEnumerable<Report>> GetByStatusAsync(string status)
    {
        return await _context.Reports
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Zamanlanmış raporları getirir
    /// </summary>
    public async Task<IEnumerable<Report>> GetScheduledReportsAsync()
    {
        return await _context.Reports
            .Where(r => r.IsScheduled && r.NextRunDate != null)
            .OrderBy(r => r.NextRunDate)
            .ToListAsync();
    }

    /// <summary>
    /// Çalıştırılması gereken raporları getirir
    /// </summary>
    public async Task<IEnumerable<Report>> GetReportsToRunAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Reports
            .Where(r => r.IsScheduled && r.NextRunDate <= now && r.Status == "Active")
            .OrderBy(r => r.NextRunDate)
            .ToListAsync();
    }
}
