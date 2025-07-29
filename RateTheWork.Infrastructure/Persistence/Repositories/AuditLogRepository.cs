using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

/// <summary>
/// Audit log kayıtları için repository implementasyonu
/// </summary>
public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Belirli bir entity için audit loglarını getirir
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, Guid entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityName == entityName && a.EntityId == entityId.ToString())
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Belirli bir kullanıcının audit loglarını getirir
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetByUserAsync
        (Guid userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AuditLogs
            .Where(a => a.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(a => a.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CreatedAt <= endDate.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Belirli bir aksiyon tipine göre audit loglarını getirir
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetByActionAsync
        (string action, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AuditLogs
            .Where(a => a.Action == action);

        if (startDate.HasValue)
            query = query.Where(a => a.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CreatedAt <= endDate.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Tarih aralığına göre audit loglarını getirir
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.AuditLogs
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
