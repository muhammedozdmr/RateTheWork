using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.JobApplication;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// İş başvurusu entity'si için repository interface'i.
/// İş başvurularına özel veritabanı işlemlerini tanımlar.
/// </summary>
public interface IJobApplicationRepository : IBaseRepository<JobApplication>
{
    /// <summary>
    /// İş ilanına ait başvuruları getirir
    /// </summary>
    /// <param name="jobPostingId">İş ilanı ID'si</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>İş başvuruları</returns>
    Task<List<JobApplication>> GetByJobPostingIdAsync(string jobPostingId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Kullanıcının başvurularını getirir
    /// </summary>
    /// <param name="applicantUserId">Başvuran kullanıcı ID'si</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>İş başvuruları</returns>
    Task<List<JobApplication>> GetByApplicantUserIdAsync(string applicantUserId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Belirli statüdeki başvuruları getirir
    /// </summary>
    /// <param name="status">Başvuru durumu</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>İş başvuruları</returns>
    Task<List<JobApplication>> GetByStatusAsync(ApplicationStatus status, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Kullanıcının belirli bir ilana başvurup başvurmadığını kontrol eder
    /// </summary>
    /// <param name="jobPostingId">İş ilanı ID'si</param>
    /// <param name="applicantUserId">Başvuran kullanıcı ID'si</param>
    /// <returns>Başvuru yapılmış mı?</returns>
    Task<bool> HasUserAppliedAsync(string jobPostingId, string applicantUserId);
    
    /// <summary>
    /// İK personeline ait başvuruları getirir
    /// </summary>
    /// <param name="hrPersonnelId">İK personeli ID'si</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>İş başvuruları</returns>
    Task<List<JobApplication>> GetByHRPersonnelIdAsync(string hrPersonnelId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Şirkete ait tüm başvuruları getirir
    /// </summary>
    /// <param name="companyId">Şirket ID'si</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>İş başvuruları</returns>
    Task<List<JobApplication>> GetByCompanyIdAsync(string companyId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Tarih aralığına göre başvuruları getirir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>İş başvuruları</returns>
    Task<List<JobApplication>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20);
}