using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

/// <summary>
/// İnceleme oylamaları için repository implementasyonu
/// </summary>
public class ReviewVoteRepository : BaseRepository<ReviewVote>, IReviewVoteRepository
{
    public ReviewVoteRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Kullanıcının bir inceleme için verdiği oyu getirir
    /// </summary>
    public async Task<ReviewVote?> GetUserVoteForReviewAsync(string userId, string? reviewId)
    {
        if (reviewId == null)
            return null;

        return await _context.ReviewVotes
            .FirstOrDefaultAsync(rv => rv.UserId == userId && rv.ReviewId == reviewId);
    }

    /// <summary>
    /// İnceleme için oyları getirir
    /// </summary>
    public async Task<List<ReviewVote>> GetVotesForReviewAsync(string? reviewId)
    {
        if (reviewId == null)
            return new List<ReviewVote>();

        return await _context.ReviewVotes
            .Where(rv => rv.ReviewId == reviewId)
            .ToListAsync();
    }

    /// <summary>
    /// Upvote sayısını getirir
    /// </summary>
    public async Task<int> GetUpvoteCountAsync(string reviewId)
    {
        return await _context.ReviewVotes
            .CountAsync(rv => rv.ReviewId == reviewId && rv.IsUpvote);
    }

    /// <summary>
    /// Downvote sayısını getirir
    /// </summary>
    public async Task<int> GetDownvoteCountAsync(string reviewId)
    {
        return await _context.ReviewVotes
            .CountAsync(rv => rv.ReviewId == reviewId && !rv.IsUpvote);
    }

    /// <summary>
    /// Kullanıcının oy verip vermediğini kontrol eder
    /// </summary>
    public async Task<bool> HasUserVotedAsync(string userId, string reviewId)
    {
        return await _context.ReviewVotes
            .AnyAsync(rv => rv.UserId == userId && rv.ReviewId == reviewId);
    }

    /// <summary>
    /// Kullanıcının birden fazla inceleme için oylarını getirir
    /// </summary>
    public async Task<Dictionary<string, bool>> GetUserVotesForReviewsAsync(string userId, List<string> reviewIds)
    {
        var votes = await _context.ReviewVotes
            .Where(rv => rv.UserId == userId && reviewIds.Contains(rv.ReviewId))
            .ToListAsync();

        return votes.ToDictionary(v => v.ReviewId, v => v.IsUpvote);
    }

    /// <summary>
    /// Oyu günceller
    /// </summary>
    public new async Task UpdateAsync(ReviewVote vote)
    {
        _context.ReviewVotes.Update(vote);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Oyu günceller (senkron)
    /// </summary>
    public void Update(ReviewVote vote)
    {
        _context.ReviewVotes.Update(vote);
    }

    /// <summary>
    /// Oyu siler
    /// </summary>
    public new async Task DeleteAsync(ReviewVote vote)
    {
        _context.ReviewVotes.Remove(vote);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Oyu siler (senkron)
    /// </summary>
    public void Delete(ReviewVote vote)
    {
        _context.ReviewVotes.Remove(vote);
    }

    /// <summary>
    /// İnceleme için oyları getirir
    /// </summary>
    public async Task<IEnumerable<ReviewVote>> GetByReviewIdAsync(string reviewId)
    {
        return await _context.ReviewVotes
            .Where(rv => rv.ReviewId == reviewId)
            // .Include(rv => rv.User) // Navigation property not available
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının verdiği oyları getirir
    /// </summary>
    public async Task<IEnumerable<ReviewVote>> GetByUserIdAsync(string userId)
    {
        return await _context.ReviewVotes
            .Where(rv => rv.UserId == userId)
            // .Include(rv => rv.Review) // Navigation property not available
            .OrderByDescending(rv => rv.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// İnceleme için oy istatistiklerini hesaplar
    /// </summary>
    public async Task<ReviewVoteStats> GetVoteStatsAsync(string reviewId)
    {
        var votes = await _context.ReviewVotes
            .Where(rv => rv.ReviewId == reviewId)
            .ToListAsync();

        return new ReviewVoteStats
        {
            TotalVotes = votes.Count, UpVotes = votes.Count(v => v.IsUpvote), DownVotes = votes.Count(v => !v.IsUpvote)
        };
    }

    /// <summary>
    /// Kullanıcının oyunu günceller veya ekler
    /// </summary>
    public async Task<ReviewVote> AddOrUpdateVoteAsync(string userId, string reviewId, bool isUpvote)
    {
        var existingVote = await GetUserVoteForReviewAsync(userId, reviewId);

        if (existingVote != null)
        {
            existingVote.IsUpvote = isUpvote;
            existingVote.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingVote;
        }
        else
        {
            var newVote = ReviewVote.Create(userId, reviewId, isUpvote);

            await _context.ReviewVotes.AddAsync(newVote);
            await _context.SaveChangesAsync();
            return newVote;
        }
    }
}

/// <summary>
/// İnceleme oy istatistikleri
/// </summary>
public class ReviewVoteStats
{
    public int TotalVotes { get; set; }
    public int UpVotes { get; set; }
    public int DownVotes { get; set; }
    public int NetVotes => UpVotes - DownVotes;
}
