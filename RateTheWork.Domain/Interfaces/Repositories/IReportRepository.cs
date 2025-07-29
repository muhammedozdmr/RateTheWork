using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Report;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Şikayet repository interface'i
/// </summary>
public interface IReportRepository : IBaseRepository<Report>
{
    /// <summary>
    /// Queryable interface sağlar
    /// </summary>
    IQueryable<Report> GetQueryable();
    /// <summary>
    /// Belirli bir entity'ye yapılan şikayetleri getirir
    /// </summary>
    Task<List<Report>> GetReportsByEntityAsync(string entityType, string entityId);
    
    /// <summary>
    /// Kullanıcının yaptığı şikayetleri getirir
    /// </summary>
    Task<List<Report>> GetReportsByUserAsync(string userId);
    
    /// <summary>
    /// Bekleyen şikayetleri getirir
    /// </summary>
    Task<List<Report>> GetPendingReportsAsync();
    
    /// <summary>
    /// Belirli durumdaki şikayetleri getirir
    /// </summary>
    Task<List<Report>> GetReportsByStatusAsync(ReportStatus status);
    
    /// <summary>
    /// Kullanıcının belirli bir entity'ye daha önce şikayet yapıp yapmadığını kontrol eder
    /// </summary>
    Task<bool> HasUserReportedEntityAsync(string userId, string entityType, string entityId);
    
    /// <summary>
    /// Belirli bir tarih aralığındaki şikayetleri getirir
    /// </summary>
    Task<List<Report>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// En çok şikayet edilen entity'leri getirir
    /// </summary>
    Task<Dictionary<string, int>> GetMostReportedEntitiesAsync(string entityType, int topCount = 10);
}