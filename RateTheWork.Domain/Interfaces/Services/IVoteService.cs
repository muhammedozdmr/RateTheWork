namespace RateTheWork.Domain.Interfaces.Services;

public interface IVoteService
{
    Task<bool> AddOrUpdateVoteAsync(string userId, string reviewId, bool isUpvote);
    Task<bool> RemoveVoteAsync(string userId, string reviewId);
    Task<(int upvotes, int downvotes)> GetVoteCountsAsync(string reviewId);
    Task<bool?> GetUserVoteAsync(string userId, string reviewId); // null=no vote, true=upvote, false=downvote
    Task RecalculateReviewScoreAsync(string reviewId);
}
