using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Notification;
using RateTheWork.Domain.Enums.Report;
using RateTheWork.Domain.Enums.Review;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Extensions;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.ValueObjects.Review;

namespace RateTheWork.Domain.Services;

/// <summary>
/// Oylama işlemleri için domain service implementasyonu
/// </summary>
public class VoteService : IVoteService
{
    private readonly IReviewDomainService _reviewDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public VoteService(IUnitOfWork unitOfWork, IReviewDomainService reviewDomainService)
    {
        _unitOfWork = unitOfWork;
        _reviewDomainService = reviewDomainService;
    }

    public async Task<bool> AddOrUpdateVoteAsync(string userId, string reviewId, bool isUpvote, VoteSource voteSource)
    {
        // Kullanıcı kontrolü
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException(nameof(User), userId);

        if (user.IsBanned)
            throw new UnauthorizedDomainActionException("Banlı kullanıcılar oy kullanamaz");

        // Yorum kontrolü
        var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
        if (review == null || !review.IsActive)
            throw new EntityNotFoundException(nameof(Review), reviewId);

        // Kullanıcı kendi yorumuna oy veremez
        if (review.UserId == userId)
            throw new BusinessRuleException("SELF_VOTE_NOT_ALLOWED",
                "Kendi yorumunuza oy veremezsiniz");

        // Mevcut oyu kontrol et
        var existingVote = await _unitOfWork.Repository<ReviewVote>()
            .GetUserVoteForReviewAsync(userId, reviewId);

        if (existingVote != null)
        {
            // Aynı oy ise bir şey yapma
            if (existingVote.IsUpvote == isUpvote)
                return false;

            // Farklı oy ise güncelle
            existingVote.IsUpvote = isUpvote;
            existingVote.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<ReviewVote>().UpdateAsync(existingVote);
        }
        else
        {
            // Yeni oy ekle
            var newVote = ReviewVote.Create(
                userId,
                reviewId,
                isUpvote,
                voteSource
            );
            await _unitOfWork.Repository<ReviewVote>().AddAsync(newVote);
        }

        // Yorum oylarını güncelle
        await UpdateReviewVoteCounts(reviewId);

        // Yorum sahibine bildirim gönder (kendi oyuna değilse)
        if (review.UserId != userId)
        {
            var voteType = isUpvote ? "beğenildi" : "beğenilmedi";
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(review.CompanyId);

            var notification = Notification.Create(
                userId: review.UserId,
                type: NotificationType.ReviewReceivesVote,
                title: $"Yorumunuz {voteType}",
                message: $"{company?.Name ?? "Şirket"} hakkındaki yorumunuz {voteType}.",
                priority: NotificationPriority.Low,
                relatedEntityType: "Review",
                relatedEntityId: reviewId
            );

            await _unitOfWork.Repository<Notification>().AddAsync(notification);
        }

        return true;
    }

    public async Task<bool> RemoveVoteAsync(string userId, string reviewId)
    {
        var existingVote = await _unitOfWork.Repository<ReviewVote>()
            .GetUserVoteForReviewAsync(userId, reviewId);

        if (existingVote == null)
            return false;
        await _unitOfWork.Repository<ReviewVote>().DeleteAsync(existingVote);

        // Yorum oylarını güncelle
        await UpdateReviewVoteCounts(reviewId);

        return true;
    }

    public async Task<(int upvotes, int downvotes)> GetVoteCountsAsync(string reviewId)
    {
        var upvotes = await _unitOfWork.Repository<ReviewVote>().GetUpvoteCountAsync(reviewId);
        var downvotes = await _unitOfWork.Repository<ReviewVote>().GetDownvoteCountAsync(reviewId);

        return (upvotes, downvotes);
    }

    public async Task<bool?> GetUserVoteAsync(string userId, string reviewId)
    {
        var vote = await _unitOfWork.Repository<ReviewVote>()
            .GetUserVoteForReviewAsync(userId, reviewId);

        return vote?.IsUpvote;
    }

    public async Task RecalculateReviewScoreAsync(string reviewId)
    {
        var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
        if (review == null)
            throw new EntityNotFoundException(nameof(Review), reviewId);

        // Review vote count'larını güncelle
        var upvoteCount = await _unitOfWork.Repository<ReviewVote>().GetUpvoteCountAsync(reviewId);
        var downvoteCount = await _unitOfWork.Repository<ReviewVote>().GetDownvoteCountAsync(reviewId);
        await _unitOfWork.Repository<Review>().UpdateReviewVoteCountsAsync(reviewId, upvoteCount, downvoteCount);

        // Güncel review'u tekrar getir
        review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
        if (review == null) return;

        // Eğer yorum çok fazla downvote aldıysa moderasyona gönder
        if (review.Downvotes > 10 && review.Downvotes > review.Upvotes * 3)
        {
            var downvoteReport = Report.Create(
                reportedEntityType: "Review",
                reportedEntityId: reviewId,
                reporterUserId: "SYSTEM",
                reason: ReportReason.Spam, // En yakın reason olarak Spam kullanıyoruz
                description: $"Yorum {review.Downvotes} downvote aldı (upvote: {review.Upvotes}). Otomatik sistem raporu."
            );

            await _unitOfWork.Repository<Report>().AddAsync(downvoteReport);
        }

        // Eğer yorum çok fazla upvote aldıysa kalite kontrolü yap
        if (review.Upvotes > 50)
        {
            var isSuspicious = await CheckVoteManipulation(reviewId);
            if (isSuspicious)
            {
                var manipulationReport = Report.Create(
                    reportedEntityType: "Review",
                    reportedEntityId: reviewId,
                    reporterUserId: "SYSTEM",
                    reason: ReportReason.Other, // Vote manipulation için Other kullanıyoruz
                    description: "Şüpheli oylama paterni tespit edildi. [Oy Manipülasyonu Şüphesi]"
                );

                await _unitOfWork.Repository<Report>().AddAsync(manipulationReport);
            }
        }
    }

    public async Task<Dictionary<string, VoteStatus>> GetBulkVoteStatusAsync(string userId, List<string> reviewIds)
    {
        var votes = await _unitOfWork.Repository<ReviewVote>()
            .GetUserVotesForReviewsAsync(userId, reviewIds);

        var voteDict = votes.ToDictionary(v => v.ReviewId, v => v);
        var result = new Dictionary<string, VoteStatus>();

        foreach (var reviewId in reviewIds)
        {
            var vote = voteDict.GetValueOrDefault(reviewId);
            if (vote != null)
            {
                result[reviewId] = new VoteStatus
                {
                    HasVoted = true, IsUpvote = vote.IsUpvote, VotedAt = vote.CreatedAt
                };
            }
            else
            {
                result[reviewId] = new VoteStatus
                {
                    HasVoted = false, IsUpvote = false, VotedAt = null
                };
            }
        }

        return result;
    }

    public async Task<bool> DetectVoteManipulationAsync(string reviewId)
    {
        var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
        if (review == null)
            return false;

        // Son 1 saatte gelen oyları kontrol et
        var recentVotes = await _unitOfWork.Repository<ReviewVote>()
            .GetAsync(v => v.ReviewId == reviewId && v.CreatedAt > DateTime.UtcNow.AddHours(-1));

        // Çok hızlı oy artışı var mı?
        if (recentVotes.Count > 20)
            return true;

        // Aynı IP'den çoklu oy kontrolü (IP bilgisi varsa)
        // TODO: IP tracking eklendiğinde implement edilecek

        // Yeni hesaplardan gelen oy oranı kontrolü
        var voterIds = recentVotes.Select(v => v.UserId).Distinct().ToList();
        var voters = await _unitOfWork.Repository<User>().GetAsync(u => voterIds.Contains(u.Id));

        var newAccountVotes = voters.Count(u => (DateTime.UtcNow - u.CreatedAt).TotalDays < 7);
        var newAccountRatio = (double)newAccountVotes / voters.Count;

        // %80'den fazla oy yeni hesaplardan geliyorsa şüpheli
        if (newAccountRatio > 0.8)
            return true;

        // Pattern analizi - Aynı kullanıcı grubunun birbirine oy vermesi
        // TODO: Daha gelişmiş pattern analizi eklenebilir

        return false;
    }

    // Private helper methods
    private async Task UpdateReviewVoteCounts(string reviewId)
    {
        // Repository'deki metodu kullan
        var upvoteCount = await _unitOfWork.Repository<ReviewVote>().GetUpvoteCountAsync(reviewId);
        var downvoteCount = await _unitOfWork.Repository<ReviewVote>().GetDownvoteCountAsync(reviewId);
        await _unitOfWork.Repository<Review>().UpdateReviewVoteCountsAsync(reviewId, upvoteCount, downvoteCount);
    }

    private async Task CheckHelpfulReviewerBadge(string userId, int upvotes, int downvotes)
    {
        // Badge kontrolü için helpfulness score hesapla
        var helpfulnessScore = _reviewDomainService.CalculateHelpfulnessScore(upvotes, downvotes, true);

        // %80 üzeri helpfulness score'a sahip 5+ yorum varsa badge kazanır
        if (helpfulnessScore < 80)
            return;

        var userReviews = await _unitOfWork.Repository<Review>().GetReviewsByUserAsync(userId, 1, int.MaxValue);
        var highQualityReviews = userReviews
            .Where(r => r.IsActive && r.Upvotes + r.Downvotes >= 10)
            .Where(r =>
            {
                var score = _reviewDomainService.CalculateHelpfulnessScore(r.Upvotes, r.Downvotes
                    , r.IsDocumentVerified);
                return score >= 80;
            })
            .Count();

        if (highQualityReviews >= 5)
        {
            // Badge service'i çağır
            // TODO: BadgeDomainService inject edilip çağrılacak
            // await _badgeDomainService.AwardBadgeAsync(userId, BadgeType.HelpfulReviewer);
        }
    }

    public async Task<bool> ToggleVoteAsync(string userId, string reviewId, bool isUpvote)
    {
        // Kullanıcı kendi yorumuna oy veremez
        var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
        if (review == null)
            throw new EntityNotFoundException(nameof(Review), reviewId);

        if (review.UserId == userId)
            throw new BusinessRuleException("Kendi yorumunuza oy veremezsiniz.");

        // Mevcut oyu kontrol et
        var existingVote = await _unitOfWork.Repository<ReviewVote>()
            .GetUserVoteForReviewAsync(userId, reviewId);

        if (existingVote != null)
        {
            if (existingVote.IsUpvote == isUpvote)
            {
                // Aynı yönde oy varsa, oyu kaldır (toggle)
                await _unitOfWork.Repository<ReviewVote>().DeleteAsync(existingVote);
                await UpdateReviewVoteCounts(reviewId);
                return false; // Oy kaldırıldı
            }
            else
            {
                // Farklı yönde oy varsa, güncelle
                existingVote.UpdateVoteType(isUpvote);
                await _unitOfWork.Repository<ReviewVote>().UpdateAsync(existingVote);
                await UpdateReviewVoteCounts(reviewId);
                return true; // Oy güncellendi
            }
        }
        else
        {
            // Yeni oy oluştur
            var newVote = ReviewVote.Create(
                userId: userId,
                reviewId: reviewId,
                isUpvote: isUpvote,
                source: VoteSource.Direct
            );

            await _unitOfWork.Repository<ReviewVote>().AddAsync(newVote);
            await UpdateReviewVoteCounts(reviewId);

            // Badge kontrolü
            await CheckHelpfulReviewerBadge(review.UserId, review.Upvotes + (isUpvote ? 1 : 0),
                review.Downvotes + (!isUpvote ? 1 : 0));

            return true; // Yeni oy eklendi
        }
    }

    public async Task<bool> CheckVoteManipulation(string reviewId)
    {
        // Son 24 saatteki oyları kontrol et
        var recentVotes = await GetRecentVotesAsync(reviewId, 24);

        // Hızlı oy artışı kontrolü - 24 saatte 20'den fazla oy şüpheli
        if (recentVotes.Count > 20)
            return true;

        // Aynı IP'den çoklu oy kontrolü (IP bilgisi varsa)
        // TODO: IP address tracking eklendiğinde implement edilecek
        // var ipGroups = recentVotes
        //     .Where(v => !string.IsNullOrEmpty(v.IpAddress))
        //     .GroupBy(v => v.IpAddress)
        //     .Where(g => g.Count() > 3); // Aynı IP'den 3'ten fazla oy

        // if (ipGroups.Any())
        //     return true;

        // Yeni hesaplardan gelen oy oranı kontrolü
        var voterIds = recentVotes.Select(v => v.UserId).Distinct().ToList();
        if (!voterIds.Any()) return false;

        var voters = await _unitOfWork.Repository<User>().GetAsync(u => voterIds.Contains(u.Id));

        var newAccountVotes = voters.Count(u => (DateTime.UtcNow - u.CreatedAt).TotalDays < 7);
        var newAccountRatio = voters.Any() ? (double)newAccountVotes / voters.Count() : 0;

        // %80'den fazla oy yeni hesaplardan geliyorsa şüpheli
        if (newAccountRatio > 0.8)
            return true;

        // Oy verme hızı kontrolü - 1 saatte 10'dan fazla oy
        var hourlyVotes = recentVotes
            .Where(v => (DateTime.UtcNow - v.CreatedAt).TotalHours <= 1)
            .Count();

        if (hourlyVotes > 10)
            return true;

        // Pattern analizi - Aynı kullanıcı grubunun birbirine oy vermesi
        // TODO: Daha gelişmiş pattern analizi eklenebilir

        return false;
    }

    /// <summary>
    /// Belirtilen süre içindeki oyları getirir
    /// </summary>
    /// <param name="reviewId">Yorum ID'si</param>
    /// <param name="hours">Kaç saat geriye bakılacağı</param>
    /// <returns>Belirtilen süre içindeki oylar</returns>
    private async Task<List<ReviewVote>> GetRecentVotesAsync(string reviewId, int hours)
    {
        var cutoffDate = DateTime.UtcNow.AddHours(-hours);

        var votes = await _unitOfWork.Repository<ReviewVote>()
            .GetAsync(v => v.ReviewId == reviewId && v.CreatedAt > cutoffDate);

        // IReadOnlyList'i List'e dönüştür
        return votes.ToList();
    }
}
