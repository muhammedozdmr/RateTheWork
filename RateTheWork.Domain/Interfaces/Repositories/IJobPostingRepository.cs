using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.JobPosting;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// İş ilanı entity'si için repository interface'i.
/// İş ilanlarına özel veritabanı işlemlerini tanımlar.
/// </summary>
public interface IJobPostingRepository : IBaseRepository<JobPosting>
{
    /// <summary>
    /// Şirkete ait iş ilanlarını getirir
    /// </summary>
    /// <param name="companyId">Şirket ID'si</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>İş ilanları</returns>
    Task<List<JobPosting>> GetByCompanyIdAsync(string companyId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// İK personeline ait iş ilanlarını getirir
    /// </summary>
    /// <param name="hrPersonnelId">İK personeli ID'si</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>İş ilanları</returns>
    Task<List<JobPosting>> GetByHRPersonnelIdAsync(string hrPersonnelId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Aktif iş ilanlarını getirir
    /// </summary>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>Aktif iş ilanları</returns>
    Task<List<JobPosting>> GetActiveJobPostingsAsync(int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Belirli statüdeki iş ilanlarını getirir
    /// </summary>
    /// <param name="status">İlan durumu</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>İş ilanları</returns>
    Task<List<JobPosting>> GetByStatusAsync(JobPostingStatus status, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Şehre göre iş ilanlarını getirir
    /// </summary>
    /// <param name="city">Şehir adı</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>İş ilanları</returns>
    Task<List<JobPosting>> GetByCityAsync(string city, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// İş tipine göre ilanları getirir
    /// </summary>
    /// <param name="jobType">İş tipi</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>İş ilanları</returns>
    Task<List<JobPosting>> GetByJobTypeAsync(JobType jobType, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Acil iş ilanlarını getirir
    /// </summary>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>Acil iş ilanları</returns>
    Task<List<JobPosting>> GetUrgentJobPostingsAsync(int page = 1, int pageSize = 20);
    
    /// <summary>
    /// İş ilanı görüntüleme sayısını artırır
    /// </summary>
    /// <param name="jobPostingId">İş ilanı ID'si</param>
    Task IncrementViewCountAsync(string jobPostingId);
}