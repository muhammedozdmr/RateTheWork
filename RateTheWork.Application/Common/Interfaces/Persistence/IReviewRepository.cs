using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Common.Interfaces.Persistence;

/// <summary>
/// Review repository interface
/// </summary>
public interface IReviewRepository : IGenericRepository<Review>
{
    Task<IReadOnlyList<Review>> GetCompanyReviewsAsync(string companyId, int pageNumber, int pageSize);
    Task<IReadOnlyList<Review>> GetUserReviewsAsync(string userId, int pageNumber, int pageSize);
    Task<IReadOnlyList<Review>> GetReportedReviewsAsync(int pageNumber, int pageSize);
    Task<Review?> GetUserReviewForCompanyAsync(string userId, string companyId, string commentType);
    Task<decimal> GetAverageRatingForCompanyAsync(string companyId);
    Task<Dictionary<string, decimal>> GetRatingDistributionAsync(string companyId);
    
    // Blockchain metodları
    Task<List<Review>> GetReviewsNotOnBlockchainAsync(int skip = 0, int take = 100);
    Task<List<Review>> GetReviewsOnBlockchainAsync(int skip = 0, int take = 100);
    Task<int> CountReviewsOnBlockchainAsync();
    Task<Review?> GetByBlockchainTransactionHashAsync(string transactionHash);
    Task<List<Review>> GetUserReviewsOnBlockchainAsync(string userId);
}