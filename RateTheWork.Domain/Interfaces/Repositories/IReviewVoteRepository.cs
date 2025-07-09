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
}
