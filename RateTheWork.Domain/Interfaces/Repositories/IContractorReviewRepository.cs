using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Contractor/Freelancer yorum repository interface
/// </summary>
public interface IContractorReviewRepository : IBaseRepository<ContractorReview>
{
    /// <summary>
    /// Şirkete ait contractor yorumlarını getirir
    /// </summary>
    Task<List<ContractorReview>> GetByCompanyIdAsync(string companyId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Kullanıcının contractor yorumlarını getirir
    /// </summary>
    Task<List<ContractorReview>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Doğrulanmış contractor yorumlarını getirir
    /// </summary>
    Task<List<ContractorReview>> GetVerifiedByCompanyIdAsync(string companyId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Tarih aralığına göre yorumları getirir
    /// </summary>
    Task<List<ContractorReview>> GetByDateRangeAsync(string companyId, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// İstatistikleri getirir
    /// </summary>
    Task<ContractorReviewStatistics> GetStatisticsAsync(string companyId);
    
    /// <summary>
    /// Kullanıcının şirkete daha önce contractor yorumu yapıp yapmadığını kontrol eder
    /// </summary>
    Task<bool> HasUserReviewedCompanyAsync(string userId, string companyId);
}

/// <summary>
/// Contractor yorum istatistikleri
/// </summary>
public class ContractorReviewStatistics
{
    public int TotalReviews { get; set; }
    public int VerifiedReviews { get; set; }
    public decimal AverageOverallRating { get; set; }
    public decimal AveragePaymentTimelinessRating { get; set; }
    public decimal AverageCommunicationRating { get; set; }
    public decimal AverageProjectManagementRating { get; set; }
    public decimal AverageTechnicalCompetenceRating { get; set; }
    public int WouldWorkAgainCount { get; set; }
    public decimal WouldWorkAgainPercentage { get; set; }
    public Dictionary<string, int> ReviewsByDuration { get; set; } = new();
    public Dictionary<string, decimal> AverageRatingsByMonth { get; set; } = new();
}