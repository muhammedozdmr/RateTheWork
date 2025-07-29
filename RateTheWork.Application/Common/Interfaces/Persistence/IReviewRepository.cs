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
}