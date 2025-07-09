using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Şirket entity'si için repository interface'i.
/// Şirketlere özel veritabanı işlemlerini tanımlar.
/// </summary>
public interface ICompanyRepository : IBaseRepository<Company>
{
    /// <summary>
    /// Vergi numarasına göre şirket arar
    /// </summary>
    /// <param name="taxId">Vergi kimlik numarası</param>
    /// <returns>Bulunan şirket veya null</returns>
    Task<Company?> GetByTaxIdAsync(string taxId);
    
    /// <summary>
    /// MERSİS numarasına göre şirket arar
    /// </summary>
    /// <param name="mersisNo">MERSİS numarası</param>
    /// <returns>Bulunan şirket veya null</returns>
    Task<Company?> GetByMersisNoAsync(string mersisNo);
    
    /// <summary>
    /// Şirket adına ve sektöre göre arama yapar
    /// </summary>
    /// <param name="searchTerm">Arama terimi</param>
    /// <param name="sector">Sektör filtresi (opsiyonel)</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>Bulunan şirketler</returns>
    Task<List<Company>> SearchCompaniesAsync(string searchTerm, string? sector = null, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Belirli bir sektördeki şirketleri getirir
    /// </summary>
    /// <param name="sector">Sektör adı</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>Sektördeki şirketler</returns>
    Task<List<Company>> GetCompaniesBySectorAsync(string sector, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Şirkete ait yorumları getirir
    /// </summary>
    /// <param name="companyId">Şirket ID'si</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <returns>Şirkete ait yorumlar</returns>
    Task<List<Review>> GetCompanyReviewsAsync(string companyId, int page = 1, int pageSize = 10);
    
    /// <summary>
    /// Onay bekleyen şirketleri getirir (admin paneli için)
    /// </summary>
    /// <returns>Onay bekleyen şirketler</returns>
    Task<List<Company>> GetPendingApprovalCompaniesAsync();
    
    /// <summary>
    /// Vergi numarasının başka bir şirket tarafından kullanılıp kullanılmadığını kontrol eder
    /// </summary>
    /// <param name="taxId">Kontrol edilecek vergi numarası</param>
    /// <returns>Vergi numarası kullanımda mı?</returns>
    Task<bool> IsTaxIdTakenAsync(string taxId);
    
    /// <summary>
    /// MERSİS numarasının başka bir şirket tarafından kullanılıp kullanılmadığını kontrol eder
    /// </summary>
    /// <param name="mersisNo">Kontrol edilecek MERSİS numarası</param>
    /// <returns>MERSİS numarası kullanımda mı?</returns>
    Task<bool> IsMersisNoTakenAsync(string mersisNo);
    
    /// <summary>
    /// Şirketin ortalama puanını yeniden hesaplar ve günceller
    /// </summary>
    /// <param name="companyId">Şirket ID'si</param>
    Task RecalculateAverageRatingAsync(string? companyId);
    
    /// <summary>
    /// Şirketin ağırlıklı ortalama puanını hesaplar
    /// (Daha yeni yorumlar daha fazla ağırlığa sahip)
    /// </summary>
    /// <param name="companyId">Şirket ID'si</param>
    /// <returns>Ağırlıklı ortalama puan</returns>
    Task<decimal> CalculateWeightedAverageRatingAsync(string companyId);
}
