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
    /// İnceleme için oyları getirir
    /// </summary>
    public async Task<IEnumerable<ReviewVote>> GetByReviewIdAsync(Guid reviewId)
    {
        return await _context.ReviewVotes
            .Where(rv => rv.ReviewId == reviewId)
            .Include(rv => rv.User)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının verdiği oyları getirir
    /// </summary>
    public async Task<IEnumerable<ReviewVote>> GetByUserIdAsync(Guid userId)
    {
        return await _context.ReviewVotes
            .Where(rv => rv.UserId == userId)
            .Include(rv => rv.Review)
            .OrderByDescending(rv => rv.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının bir inceleme için verdiği oyu getirir
    /// </summary>
    public async Task<ReviewVote?> GetUserVoteForReviewAsync(Guid userId, Guid reviewId)
    {
        return await _context.ReviewVotes
            .FirstOrDefaultAsync(rv => rv.UserId == userId && rv.ReviewId == reviewId);
    }

    /// <summary>
    /// İnceleme için oy istatistiklerini hesaplar
    /// </summary>
    public async Task<ReviewVoteStats> GetVoteStatsAsync(Guid reviewId)
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
    public async Task<ReviewVote> AddOrUpdateVoteAsync(Guid userId, Guid reviewId, bool isUpvote)
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
            var newVote = new ReviewVote
            {
                UserId = userId, ReviewId = reviewId, IsUpvote = isUpvote
            };

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
