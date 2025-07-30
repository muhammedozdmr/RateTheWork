using Microsoft.EntityFrameworkCore;
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
    public async Task<List<AuditLog>> GetByEntityAsync
        (string entityType, string entityId, int page = 1, int pageSize = 50)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityName == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Belirli bir admin kullanıcısının audit loglarını getirir
    /// </summary>
    public async Task<List<AuditLog>> GetByAdminUserIdAsync(string adminUserId, int page = 1, int pageSize = 50)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == adminUserId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Belirli bir aksiyon tipindeki audit loglarını getirir
    /// </summary>
    public async Task<List<AuditLog>> GetByActionTypeAsync(string actionType, int page = 1, int pageSize = 50)
    {
        return await _context.AuditLogs
            .Where(a => a.Action == actionType)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Belirli bir tarih aralığındaki audit loglarını getirir
    /// </summary>
    public async Task<List<AuditLog>> GetByDateRangeAsync
        (DateTime startDate, DateTime endDate, int page = 1, int pageSize = 50)
    {
        return await _context.AuditLogs
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
