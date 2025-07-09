namespace RateTheWork.Domain.Interfaces.Services;

/// <summary>
/// Oylama işlemleri için domain service interface'i
/// </summary>
public interface IVoteService
{
    /// <summary>
    /// Oy ekler veya günceller
    /// </summary>
    Task<bool> AddOrUpdateVoteAsync(string userId, string reviewId, bool isUpvote);
    
    /// <summary>
    /// Oyu kaldırır
    /// </summary>
    Task<bool> RemoveVoteAsync(string userId, string reviewId);
    
    /// <summary>
    /// Oy sayılarını getirir
    /// </summary>
    Task<(int upvotes, int downvotes)> GetVoteCountsAsync(string reviewId);
    
    /// <summary>
    /// Kullanıcının oyunu getirir
    /// </summary>
    Task<bool?> GetUserVoteAsync(string userId, string reviewId);
    
    /// <summary>
    /// Yorum skorunu yeniden hesaplar
    /// </summary>
    Task RecalculateReviewScoreAsync(string reviewId);
    
    /// <summary>
    /// Toplu oy durumunu getirir
    /// </summary>
    Task<Dictionary<string, VoteStatus>> GetBulkVoteStatusAsync(string userId, List<string> reviewIds);
    
    /// <summary>
    /// Oy manipülasyonu kontrolü
    /// </summary>
    Task<bool> DetectVoteManipulationAsync(string reviewId);
}
