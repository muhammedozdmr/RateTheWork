using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces;

public interface IReviewRepository : IBaseRepository<Review>
{
    Task<List<Review>> GetReviewsByCompanyAsync(string companyId, int page = 1, int pageSize = 10);
    Task<List<Review>> GetReviewsByUserAsync(string userId, int page = 1, int pageSize = 10);
    Task<List<Review>> GetReviewsByTypeAsync(string companyId, string commentType, int page = 1, int pageSize = 10);
    Task<List<Review>> GetPendingVerificationReviewsAsync();
    Task<List<Report>> GetReviewReportsAsync(string? reviewId);
    Task<decimal> GetCompanyAverageRatingAsync(string companyId);
    Task<int> GetCompanyReviewCountAsync(string companyId);
    Task UpdateReviewVoteCountsAsync(string? reviewId); // Upvote/Downvote count'larını güncelle
}
