using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.CVApplication;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// CV başvuru repository interface
/// </summary>
public interface ICVApplicationRepository : IBaseRepository<CVApplication>
{
    /// <summary>
    /// Kullanıcının başvurularını getirir
    /// </summary>
    Task<List<CVApplication>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Şirketin CV havuzunu getirir
    /// </summary>
    Task<List<CVApplication>> GetByCompanyIdAsync(string companyId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Departmana göre başvuruları getirir
    /// </summary>
    Task<List<CVApplication>> GetByDepartmentIdAsync(string departmentId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Duruma göre başvuruları getirir
    /// </summary>
    Task<List<CVApplication>> GetByStatusAsync(CVApplicationStatus status, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Süresi dolmuş başvuruları getirir
    /// </summary>
    Task<List<CVApplication>> GetExpiredApplicationsAsync(DateTime asOfDate);
    
    /// <summary>
    /// Geri bildirim süresi dolmuş başvuruları getirir
    /// </summary>
    Task<List<CVApplication>> GetFeedbackOverdueApplicationsAsync(DateTime asOfDate);
    
    /// <summary>
    /// Kullanıcının şirkete daha önce başvuru yapıp yapmadığını kontrol eder
    /// </summary>
    Task<bool> HasUserAppliedToCompanyAsync(string userId, string companyId, DateTime withinDays);
    
    /// <summary>
    /// İstatistikleri getirir
    /// </summary>
    Task<CVApplicationStatistics> GetStatisticsAsync(string companyId);
}

/// <summary>
/// CV başvuru istatistikleri
/// </summary>
public class CVApplicationStatistics
{
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int ViewedApplications { get; set; }
    public int DownloadedApplications { get; set; }
    public int RespondedApplications { get; set; }
    public decimal AverageResponseTime { get; set; } // Gün cinsinden
    public Dictionary<string, int> ApplicationsByDepartment { get; set; } = new();
    public Dictionary<CVApplicationStatus, int> ApplicationsByStatus { get; set; } = new();
}