using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Audit log entity'si için repository interface'i.
/// Denetim kayıtlarına özel veritabanı işlemlerini tanımlar.
/// </summary>
public interface IAuditLogRepository : IBaseRepository<AuditLog>
{
    /// <summary>
    /// Belirli bir kullanıcının audit loglarını getirir
    /// </summary>
    /// <param name="adminUserId">Admin kullanıcı ID'si</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>Audit logları</returns>
    Task<List<AuditLog>> GetByAdminUserIdAsync(string adminUserId, int page = 1, int pageSize = 50);
    
    /// <summary>
    /// Belirli bir entity için audit loglarını getirir
    /// </summary>
    /// <param name="entityType">Entity tipi</param>
    /// <param name="entityId">Entity ID'si</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>Audit logları</returns>
    Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId, int page = 1, int pageSize = 50);
    
    /// <summary>
    /// Belirli bir tarih aralığındaki audit loglarını getirir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>Audit logları</returns>
    Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 50);
    
    /// <summary>
    /// Belirli bir aksiyon tipindeki audit loglarını getirir
    /// </summary>
    /// <param name="actionType">Aksiyon tipi</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>Audit logları</returns>
    Task<List<AuditLog>> GetByActionTypeAsync(string actionType, int page = 1, int pageSize = 50);
}