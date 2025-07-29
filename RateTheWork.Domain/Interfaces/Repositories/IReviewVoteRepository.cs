using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Repositories;

public interface IReviewVoteRepository : IBaseRepository<ReviewVote>
{
    Task<ReviewVote?> GetUserVoteForReviewAsync(string userId, string? reviewId);
    Task<List<ReviewVote>> GetVotesForReviewAsync(string? reviewId);
    Task<int> GetUpvoteCountAsync(string reviewId);
    Task<int> GetDownvoteCountAsync(string reviewId);
    Task<bool> HasUserVotedAsync(string userId, string reviewId);
    Task<Dictionary<string, bool>> GetUserVotesForReviewsAsync(string userId, List<string> reviewIds);
    
    /// <summary>
    /// Oyu günceller
    /// </summary>
    Task UpdateAsync(ReviewVote vote);
    
    /// <summary>
    /// Oyu günceller (senkron)
    /// </summary>
    void Update(ReviewVote vote);
    
    /// <summary>
    /// Oyu siler
    /// </summary>
    Task DeleteAsync(ReviewVote vote);
    
    /// <summary>
    /// Oyu siler (senkron)
    /// </summary>
    void Delete(ReviewVote vote);
}

// Extension metodları için
public static class ReviewVoteRepositoryExtensions
{
    public static Task<Dictionary<string?, bool>> GetUserVotesForReviewsAsync(this IReviewVoteRepository repo, string userId, List<string?> reviewIds)
    {
        var nonNullIds = reviewIds.Where(id => id != null).Select(id => id!).ToList();
        return repo.GetUserVotesForReviewsAsync(userId, nonNullIds);
    }
}
