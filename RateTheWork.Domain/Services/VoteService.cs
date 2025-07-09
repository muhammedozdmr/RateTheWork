using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;

namespace RateTheWork.Domain.Services;

//TODO: IsUpvote private set entitylerde UpdatedAt prop yok UpdateAsync ve DeleteAsync repoda yok targetType targetId CreatedAt prop yok

/// <summary>
/// Oylama işlemleri için domain service implementasyonu
/// </summary>
public class VoteService : IVoteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReviewDomainService _reviewDomainService;

    public VoteService(IUnitOfWork unitOfWork, IReviewDomainService reviewDomainService)
    {
        _unitOfWork = unitOfWork;
        _reviewDomainService = reviewDomainService;
    }

    public async Task<bool> AddOrUpdateVoteAsync(string userId, string reviewId, bool isUpvote)
    {
        // Kullanıcı kontrolü
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException(nameof(User), userId);
        
        if (user.IsBanned)
            throw new UnauthorizedDomainActionException("Banlı kullanıcılar oy kullanamaz");

        // Yorum kontrolü
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review == null || !review.IsActive)
            throw new EntityNotFoundException(nameof(Review), reviewId);

        // Kullanıcı kendi yorumuna oy veremez
        if (review.UserId == userId)
            throw new BusinessRuleException("SELF_VOTE_NOT_ALLOWED", 
                "Kendi yorumunuza oy veremezsiniz");

        // Mevcut oyu kontrol et
        var existingVote = await _unitOfWork.ReviewVotes
            .GetUserVoteForReviewAsync(userId, reviewId);

        if (existingVote != null)
        {
            // Aynı oy ise bir şey yapma
            if (existingVote.IsUpvote == isUpvote)
                return false;

            // Farklı oy ise güncelle
            existingVote.IsUpvote = isUpvote;
            existingVote.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.ReviewVotes.UpdateAsync(existingVote);
        }
        else
        {
            // Yeni oy ekle
            var newVote = ReviewVote.Create(reviewId, userId, isUpvote);
            await _unitOfWork.ReviewVotes.AddAsync(newVote);
        }

        // Yorum oylarını güncelle
        await UpdateReviewVoteCounts(reviewId);

        // Yorum sahibine bildirim gönder (kendi oyuna değilse)
        if (review.UserId != userId)
        {
            var voteType = isUpvote ? "beğenildi" : "beğenilmedi";
            var company = await _unitOfWork.Companies.GetByIdAsync(review.CompanyId);
            
            var notification = Notification.Create(
                userId: review.UserId,
                type: NotificationType.ReviewReceivesVote,
                title: $"Yorumunuz {voteType}",
                message: $"{company?.Name ?? "Şirket"} hakkındaki yorumunuz {voteType}.",
                priority: NotificationPriority.Low,
                relatedEntityType: "Review",
                relatedEntityId: reviewId
            );
            
            await _unitOfWork.Notifications.AddAsync(notification);
        }

        return true;
    }

    public async Task<bool> RemoveVoteAsync(string userId, string reviewId)
    {
        var existingVote = await _unitOfWork.ReviewVotes
            .GetUserVoteForReviewAsync(userId, reviewId);

        if (existingVote == null)
            return false;

        await _unitOfWork.ReviewVotes.DeleteAsync(existingVote.Id);

        // Yorum oylarını güncelle
        await UpdateReviewVoteCounts(reviewId);

        return true;
    }

    public async Task<(int upvotes, int downvotes)> GetVoteCountsAsync(string reviewId)
    {
        var upvotes = await _unitOfWork.ReviewVotes.GetUpvoteCountAsync(reviewId);
        var downvotes = await _unitOfWork.ReviewVotes.GetDownvoteCountAsync(reviewId);

        return (upvotes, downvotes);
    }

    public async Task<bool?> GetUserVoteAsync(string userId, string reviewId)
    {
        var vote = await _unitOfWork.ReviewVotes
            .GetUserVoteForReviewAsync(userId, reviewId);

        if (vote == null)
            return null;

        return vote.IsUpvote;
    }

    public async Task RecalculateReviewScoreAsync(string reviewId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review == null)
            throw new EntityNotFoundException(nameof(Review), reviewId);

        // Review vote count'larını güncelle
        await _unitOfWork.Reviews.UpdateReviewVoteCountsAsync(reviewId);

        // Güncel review'u tekrar getir
        review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review == null) return;

        // Eğer yorum çok fazla downvote aldıysa moderasyona gönder
        if (review.Downvotes > 10 && review.Downvotes > review.Upvotes * 3)
        {
            var report = Report.Create(
                reporterUserId: "SYSTEM",
                targetType: "Review",
                targetId: reviewId,
                reportReason: "Yüksek Downvote Oranı",
                reportDetails: $"Yorum {review.Downvotes} downvote aldı (upvote: {review.Upvotes}). Otomatik sistem raporu."
            );
            
            await _unitOfWork.Reports.AddAsync(report);
        }
    }

    public async Task<Dictionary<string, VoteStatus>> GetBulkVoteStatusAsync(string userId, List<string> reviewIds)
    {
        var votes = await _unitOfWork.ReviewVotes
            .GetUserVotesForReviewsAsync(userId, reviewIds);

        var result = new Dictionary<string, VoteStatus>();

        foreach (var reviewId in reviewIds)
        {
            var vote = votes.GetValueOrDefault(reviewId);
            result[reviewId] = new VoteStatus
            {
                HasVoted = vote != null,
                IsUpvote = vote?.IsUpvote,
                VotedAt = vote?.CreatedAt
            };
        }

        return result;
    }

    public async Task<bool> DetectVoteManipulationAsync(string reviewId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review == null)
            return false;

        // Son 1 saatte gelen oyları kontrol et
        var recentVotes = await _unitOfWork.ReviewVotes
            .GetAsync(v => v.ReviewId == reviewId && v.CreatedAt > DateTime.UtcNow.AddHours(-1));

        // Çok hızlı oy artışı var mı?
        if (recentVotes.Count > 20)
            return true;

        // Aynı IP'den çoklu oy kontrolü (IP bilgisi varsa)
        // TODO: IP tracking eklendiğinde implement edilecek

        // Yeni hesaplardan gelen oy oranı kontrolü
        var voterIds = recentVotes.Select(v => v.UserId).Distinct().ToList();
        var voters = await _unitOfWork.Users.GetAsync(u => voterIds.Contains(u.Id));
        
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
        await _unitOfWork.Reviews.UpdateReviewVoteCountsAsync(reviewId);
    }

    private async Task CheckHelpfulReviewerBadge(string userId, int upvotes, int downvotes)
    {
        // Badge kontrolü için helpfulness score hesapla
        var helpfulnessScore = _reviewDomainService.CalculateHelpfulnessScore(upvotes, downvotes, true);
        
        // %80 üzeri helpfulness score'a sahip 5+ yorum varsa badge kazanır
        if (helpfulnessScore < 80)
            return;

        var userReviews = await _unitOfWork.Reviews.GetReviewsByUserAsync(userId, 1, int.MaxValue);
        var highQualityReviews = userReviews
            .Where(r => r.IsActive && r.Upvotes + r.Downvotes >= 10)
            .Where(r => 
            {
                var score = _reviewDomainService.CalculateHelpfulnessScore(r.Upvotes, r.Downvotes, r.IsDocumentVerified);
                return score >= 80;
            })
            .Count();

        if (highQualityReviews >= 5)
        {
            // Badge service'i çağır
            // TODO: BadgeDomainService inject edilip çağrılacak
        }
    }
}
