using System.Linq.Expressions;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Badge;
using RateTheWork.Domain.Enums.Report;
using RateTheWork.Domain.Enums.VerificationRequest;
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

    // Generic Repository Extensions
    public static async Task<T?> GetByIdIncludingAsync<T>
    (
        this IRepository<T> repository
        , string id
        , params Expression<Func<T, object>>[] includes
    ) where T : BaseEntity
    {
        // Include işlemi için infrastructure katmanında özel implementasyon gerekecek
        // Şimdilik standart GetByIdAsync kullanıyoruz
        return await repository.GetByIdAsync(id);
    }

    public static async Task<bool> ExistsAsync<T>
    (
        this IRepository<T> repository
        , Expression<Func<T, bool>> predicate
    ) where T : BaseEntity
    {
        return await repository.CountAsync(predicate) > 0;
    }

    public static async Task<T?> GetLatestAsync<T>
    (
        this IRepository<T> repository
        , Expression<Func<T, bool>>? predicate = null
    ) where T : BaseEntity
    {
        var query = await repository.GetAsync(predicate ?? (x => true));
        return query.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
    }

    public static async Task<List<T>> GetRecentAsync<T>
    (
        this IRepository<T> repository
        , int count
        , Expression<Func<T, bool>>? predicate = null
    ) where T : BaseEntity
    {
        var query = await repository.GetAsync(predicate ?? (x => true));
        return query.OrderByDescending(x => x.CreatedAt).Take(count).ToList();
    }

    // Company Repository Extensions
    public static async Task<List<Company>> GetCompaniesBySectorAsync
    (
        this IRepository<Company> repository
        , string sector
        , bool onlyApproved = true
    )
    {
        return (await repository.GetAsync(c =>
            c.Sector.ToString() == sector &&
            (!onlyApproved || c.IsApproved))).ToList();
    }

    public static async Task<List<Company>> GetTopRatedCompaniesAsync
    (
        this IRepository<Company> repository
        , int count = 10
    )
    {
        var companies = await repository.GetAsync(c => c.IsApproved);
        return companies
            .OrderByDescending(c => c.ReviewStatistics.AverageRating)
            .ThenByDescending(c => c.ReviewStatistics.TotalReviews)
            .Take(count)
            .ToList();
    }

    // User Repository Extensions
    public static async Task<User?> GetByEmailAsync
    (
        this IRepository<User> repository
        , string email
    )
    {
        return await repository.GetFirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public static async Task<bool> IsEmailUniqueAsync
    (
        this IRepository<User> repository
        , string email
        , string? excludeUserId = null
    )
    {
        return !await repository.ExistsAsync(u =>
            u.Email.ToLower() == email.ToLower() &&
            (excludeUserId == null || u.Id != excludeUserId));
    }

    public static async Task<List<User>> GetActiveUsersAsync
    (
        this IRepository<User> repository
        , int page
        , int size
    )
    {
        var (items, _) = await repository.GetPagedAsync(
            u => u.IsActive && !u.IsBanned && u.IsEmailVerified,
            page,
            size);
        return items.ToList();
    }

    // Report Repository Extensions
    public static async Task<List<Report>> GetPendingReportsAsync
    (
        this IRepository<Report> repository
    )
    {
        return (await repository.GetAsync(r =>
            r.Status == ReportStatus.Pending)).ToList();
    }

    public static async Task<List<Report>> GetReportsByTargetAsync
    (
        this IRepository<Report> repository
        , string targetId
        , string targetType
    )
    {
        return (await repository.GetAsync(r =>
            r.TargetId == targetId &&
            r.TargetType == targetType)).ToList();
    }

    // Badge Repository Extensions
    public static async Task<List<Badge>> GetBadgesByTypeAsync
    (
        this IRepository<Badge> repository
        , BadgeType badgeType
    )
    {
        return (await repository.GetAsync(b =>
            b.Type == badgeType && b.IsActive)).ToList();
    }

    public static async Task<List<Badge>> GetActiveBadgesAsync
    (
        this IRepository<Badge> repository
    )
    {
        var now = DateTime.UtcNow;
        return (await repository.GetAsync(b =>
            b.IsActive &&
            (!b.AvailableFrom.HasValue || b.AvailableFrom.Value <= now) &&
            (!b.AvailableUntil.HasValue || b.AvailableUntil.Value >= now))).ToList();
    }

    // TODO: BadgeProgress entity oluşturulduktan sonra aşağıdaki extension'lar eklenecek
    // BadgeProgress Repository Extensions
    // GetUserProgressAsync() - Kullanıcının rozet ilerlemelerini getirir
    // GetProgressByBadgeTypeAsync() - Belirli tip rozet ilerlemesini getirir

    // VerificationRequest Repository Extensions
    public static async Task<List<VerificationRequest>> GetPendingVerificationRequestsAsync
    (
        this IRepository<VerificationRequest> repository
    )
    {
        return (await repository.GetAsync(vr =>
            vr.Status == VerificationRequestStatus.Pending)).ToList();
    }

    public static async Task<VerificationRequest?> GetLatestVerificationRequestAsync
    (
        this IRepository<VerificationRequest> repository
        , string userId
        , string companyId
    )
    {
        var requests = await repository.GetAsync(vr =>
            vr.UserId == userId && vr.CompanyId == companyId);
        return requests.OrderByDescending(vr => vr.CreatedAt).FirstOrDefault();
    }
}
