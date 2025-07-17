using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Domain.Extensions;

/// <summary>
/// Repository extension methods
/// </summary>
public static class RepositoryExtensions
{
    // ReviewVote Repository Extensions
    public static async Task<ReviewVote?> GetUserVoteForReviewAsync
        (this IRepository<ReviewVote> repository, string userId, string reviewId)
    {
        return await repository.GetFirstOrDefaultAsync(rv => rv.UserId == userId && rv.ReviewId == reviewId);
    }

    public static async Task<int> GetUpvoteCountAsync(this IRepository<ReviewVote> repository, string reviewId)
    {
        return await repository.CountAsync(rv => rv.ReviewId == reviewId && rv.IsUpvote);
    }

    public static async Task<int> GetDownvoteCountAsync(this IRepository<ReviewVote> repository, string reviewId)
    {
        return await repository.CountAsync(rv => rv.ReviewId == reviewId && !rv.IsUpvote);
    }

    public static async Task<List<ReviewVote>> GetUserVotesForReviewsAsync
        (this IRepository<ReviewVote> repository, string userId, List<string> reviewIds)
    {
        var votes = await repository.GetAsync(rv => rv.UserId == userId && reviewIds.Contains(rv.ReviewId));
        return votes.ToList();
    }

    // Review Repository Extensions
    public static async Task<List<Review>> GetReviewsByUserAsync
        (this IRepository<Review> repository, string userId, int page, int size)
    {
        var (items, _) = await repository.GetPagedAsync(r => r.UserId == userId, page, size);
        return items.ToList();
    }

    public static async Task<List<Review>> GetReviewsByCompanyAsync
        (this IRepository<Review> repository, string companyId, int page, int size)
    {
        var (items, _) = await repository.GetPagedAsync(r => r.CompanyId == companyId, page, size);
        return items.ToList();
    }

    public static async Task UpdateReviewVoteCountsAsync
        (this IRepository<Review> repository, string reviewId, int upvoteCount, int downvoteCount)
    {
        var review = await repository.GetByIdAsync(reviewId);
        if (review != null)
        {
            // Review entity'de vote count property'leri varsa update et
            // Şimdilik boş bırakıyoruz, infrastructure katmanında implement edilecek
            await repository.UpdateAsync(review);
        }
    }
}
