using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Review;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

public class ReviewRepository : BaseRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Review>> GetReviewsByCompanyAsync(string companyId, int page = 1, int pageSize = 10)
    {
        return await _dbSet
            .Where(r => r.CompanyId == companyId.ToString() && r.IsActive && r.IsPublished)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Review>> GetReviewsByUserAsync(string userId, int page = 1, int pageSize = 10)
    {
        return await _dbSet
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Review>> GetReviewsByTypeAsync
        (string companyId, string commentType, int page = 1, int pageSize = 10)
    {
        var query = _dbSet
            .Where(r => r.CompanyId == companyId.ToString() && r.IsActive && r.IsPublished);

        // Filter by comment type
        if (Enum.TryParse<CommentType>(commentType, true, out var type))
        {
            query = query.Where(r => r.CommentType == type);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Review>> GetPendingVerificationReviewsAsync()
    {
        return await _dbSet
            .Where(r => r.IsActive && !r.IsPublished)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Report>> GetReviewReportsAsync(string? reviewId)
    {
        // Report entity is not part of Review aggregate
        return new List<Report>();
    }

    public async Task<decimal> GetCompanyAverageRatingAsync(string companyId)
    {
        var averageRating = await _dbSet
            .Where(r => r.CompanyId == companyId.ToString() && r.IsActive && r.IsPublished)
            .AverageAsync(r => (decimal?)r.OverallRating);

        return averageRating ?? 0;
    }

    public async Task<int> GetCompanyReviewCountAsync(string companyId)
    {
        return await _dbSet
            .CountAsync(r => r.CompanyId == companyId.ToString() && r.IsActive && r.IsPublished);
    }

    public async Task UpdateReviewVoteCountsAsync(string? reviewId)
    {
        if (string.IsNullOrEmpty(reviewId))
            return;

        var review = await _dbSet.FirstOrDefaultAsync(r => r.Id == reviewId);
        if (review != null)
        {
            // Votes should be counted from ReviewVote repository
            // Review entity has Upvotes and Downvotes properties but they need to be updated via domain methods
            await _context.SaveChangesAsync();
        }
    }

    public IQueryable<Review> GetQueryable()
    {
        return _dbSet;
    }

    async Task IReviewRepository.UpdateAsync(Review review)
    {
        _dbSet.Update(review);
        await _context.SaveChangesAsync();
    }

    public void Update(Review review)
    {
        _dbSet.Update(review);
    }

    public async Task<int> GetCountAsync(Func<Review, bool> predicate)
    {
        return await Task.FromResult(_dbSet.Where(predicate).Count());
    }

    // Additional helper methods for backward compatibility
    public async Task<bool> HasUserReviewedCompanyAsync
        (Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(
                r => r.UserId == userId.ToString() && r.CompanyId == companyId.ToString().ToString() && r.IsActive &&
                     r.IsPublished, cancellationToken);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync
        (Guid companyId, CancellationToken cancellationToken = default)
    {
        var distribution = await _dbSet
            .Where(r => r.CompanyId == companyId.ToString() && r.IsActive && r.IsPublished)
            .GroupBy(r => r.OverallRating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var result = new Dictionary<int, int>();
        for (int i = 1; i <= 5; i++)
        {
            result[i] = distribution.FirstOrDefault(d => d.Rating == i)?.Count ?? 0;
        }

        return result;
    }

    public async Task<IEnumerable<Review>> GetRecentReviewsAsync
        (int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.IsActive && r.IsPublished)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<Review?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Review>> GetByBranchAsync
        (Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.IsActive && r.IsPublished)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
