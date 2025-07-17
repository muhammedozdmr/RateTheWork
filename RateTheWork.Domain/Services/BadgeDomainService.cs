using RateTheWork.Domain.Constants;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Badge;
using RateTheWork.Domain.Enums.Notification;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Services;

/// <summary>
/// Rozet işlemleri için domain service implementasyonu
/// </summary>
public class BadgeDomainService : IBadgeDomainService
{
    private readonly IUnitOfWork _unitOfWork;

    public BadgeDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Badge>> CheckEligibleBadgesAsync(string userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException(nameof(User), userId);

        var allBadges = await _unitOfWork.Repository<Badge>().GetAllAsync();
        var userBadges = await _unitOfWork.Repository<UserBadge>().GetAsync(ub => ub.UserId == userId);
        var earnedBadgeIds = userBadges.Select(ub => ub.BadgeId).ToHashSet();

        var eligibleBadges = new List<Badge>();

        foreach (var badge in allBadges.Where(b => !earnedBadgeIds.Contains(b.Id) && b.IsActive))
        {
            if (await CheckBadgeCriteriaAsync(userId, badge))
            {
                eligibleBadges.Add(badge);
            }
        }

        return eligibleBadges;
    }

    public async Task AwardBadgeAsync(string userId, string badgeId)
    {
        // Kullanıcı kontrolü
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException(nameof(User), userId);

        if (user.IsBanned)
            throw new BusinessRuleException("BANNED_USER", "Banlı kullanıcılar rozet kazanamaz.");

        // Rozet kontrolü
        var badge = await _unitOfWork.Repository<Badge>().GetByIdAsync(badgeId);
        if (badge == null || !badge.IsActive)
            throw new EntityNotFoundException(nameof(Badge), badgeId);

        // Zaten kazanılmış mı kontrolü
        var existingUserBadge = await _unitOfWork.Repository<UserBadge>()
            .GetFirstOrDefaultAsync(ub => ub.UserId == userId && ub.BadgeId == badgeId);

        if (existingUserBadge != null)
            throw new BusinessRuleException("USER_ALREADY_HAS_BADGE",
                $"Kullanıcı bu rozete zaten sahip: {badge.Name}");

        // Kriterleri karşılıyor mu kontrolü
        if (!await CheckBadgeCriteriaAsync(userId, badge))
            throw new BusinessRuleException("BADGE_CRITERIA_NOT_MET",
                $"Kullanıcı rozet kriterlerini karşılamıyor: {badge.Name}");

        // Rozeti ata
        var userBadge = UserBadge.Award(
            userId: userId,
            badgeId: badgeId,
            awardReason: $"Automatically awarded for meeting criteria: {badge.Criteria}",
            specialNote: null
        );

        await _unitOfWork.Repository<UserBadge>().AddAsync(userBadge);

        // Kullanıcıya bildirim gönder
        var notification = Notification.Create(
            userId: userId,
            type: NotificationType.BadgeEarned,
            title: "Yeni Rozet Kazandınız!",
            message: $"Tebrikler! '{badge.Name}' rozetini kazandınız: {badge.Description}",
            priority: NotificationPriority.High,
            relatedEntityType: "Badge",
            relatedEntityId: badgeId,
            imageUrl: badge.IconUrl
        );

        await _unitOfWork.Repository<Notification>().AddAsync(notification);
    }

    public async Task<bool> CheckBadgeCriteriaAsync(string userId, Badge badge)
    {
        var userReviews = await _unitOfWork.Repository<Review>().GetReviewsByUserAsync(userId, 1, int.MaxValue);
        var verifiedReviews = userReviews.Where(r => r.IsDocumentVerified && r.IsActive).ToList();
        var activeReviews = userReviews.Where(r => r.IsActive).ToList();

        // Badge tipine göre kontrol
        switch (badge.Type)
        {
            case BadgeType.FirstReview:
                return activeReviews.Any();

            case BadgeType.ActiveReviewer:
                return activeReviews.Count >= DomainConstants.Badge.ActiveReviewerThreshold; // 10

            case BadgeType.TrustedReviewer:
                return verifiedReviews.Count >= DomainConstants.Badge.TrustedReviewerThreshold; // 5

            case BadgeType.TopContributor:
                return activeReviews.Count >= DomainConstants.Badge.TopContributorThreshold; // 50

            case BadgeType.CompanyExplorer:
                var uniqueCompanies = activeReviews
                    .Select(r => r.CompanyId)
                    .Distinct()
                    .Count();
                return uniqueCompanies >= DomainConstants.Badge.CompanyExplorerThreshold; // 10

            case BadgeType.DetailedReviewer:
                if (activeReviews.Count < 5) return false;
                var avgLength = activeReviews.Average(r => r.CommentText.Length);
                return avgLength >= DomainConstants.Review.MinCharactersForDetailedReviewer; // 500

            case BadgeType.HelpfulReviewer:
                var helpfulReviews = activeReviews
                    .Where(r => r.Upvotes + r.Downvotes >= 10) // En az 10 oy almış
                    .Where(r => r.HelpfulnessScore >= DomainConstants.Badge.HelpfulReviewerPercentage) // %80
                    .Count();
                return helpfulReviews >= 5;

            case BadgeType.Anniversary:
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
                if (user == null) return false;
                var membershipDays = (DateTime.UtcNow - user.CreatedAt).TotalDays;
                return membershipDays >= 365; // 1 yıl

            case BadgeType.SpecialEvent:
                // Özel etkinlik rozetleri manuel olarak verilir
                return false;

            default:
                return false;
        }
    }

    public async Task<Dictionary<string, BadgeProgress>> GetUserBadgeProgressAsync(string userId)
    {
        var progress = new Dictionary<string, BadgeProgress>();
        var allBadges = await _unitOfWork.Repository<Badge>().GetAllAsync();
        var userBadges = await _unitOfWork.Repository<UserBadge>().GetAsync(ub => ub.UserId == userId);
        var earnedBadgeIds = userBadges.Select(ub => ub.BadgeId).ToHashSet();

        var userReviews = await _unitOfWork.Repository<Review>().GetReviewsByUserAsync(userId, 1, int.MaxValue);
        var activeReviews = userReviews.Where(r => r.IsActive).ToList();
        var verifiedReviews = userReviews.Where(r => r.IsDocumentVerified && r.IsActive).ToList();

        foreach (var badge in allBadges.Where(b => b.IsActive))
        {
            var badgeProgress = new BadgeProgress
            {
                BadgeId = badge.Id, BadgeName = badge.Name, IsEarned = earnedBadgeIds.Contains(badge.Id)
            };

            if (!badgeProgress.IsEarned)
            {
                // İlerleme hesaplama
                switch (badge.Type)
                {
                    case BadgeType.FirstReview:
                        badgeProgress.ProgressPercentage = activeReviews.Any() ? 100 : 0;
                        badgeProgress.CurrentStatus = activeReviews.Any() ? "Tamamlandı!" : "İlk yorumunuzu yapın";
                        badgeProgress.NextRequirement = activeReviews.Any() ? "" : "1 yorum yapmanız gerekiyor";
                        break;

                    case BadgeType.ActiveReviewer:
                        var activeCount = activeReviews.Count;
                        badgeProgress.ProgressPercentage = Math.Min(100
                            , (activeCount * 100m) / DomainConstants.Badge.ActiveReviewerThreshold);
                        badgeProgress.CurrentStatus = $"{activeCount}/10 yorum";
                        badgeProgress.NextRequirement = $"{Math.Max(0, 10 - activeCount)} yorum daha yapın";
                        break;

                    case BadgeType.TrustedReviewer:
                        var verifiedCount = verifiedReviews.Count;
                        badgeProgress.ProgressPercentage = Math.Min(100
                            , (verifiedCount * 100m) / DomainConstants.Badge.TrustedReviewerThreshold);
                        badgeProgress.CurrentStatus = $"{verifiedCount}/5 doğrulanmış yorum";
                        badgeProgress.NextRequirement = $"{Math.Max(0, 5 - verifiedCount)} doğrulanmış yorum daha";
                        break;

                    case BadgeType.CompanyExplorer:
                        var uniqueCompanies = activeReviews.Select(r => r.CompanyId).Distinct().Count();
                        badgeProgress.ProgressPercentage = Math.Min(100
                            , (uniqueCompanies * 100m) / DomainConstants.Badge.CompanyExplorerThreshold);
                        badgeProgress.CurrentStatus = $"{uniqueCompanies}/10 farklı şirket";
                        badgeProgress.NextRequirement =
                            $"{Math.Max(0, 10 - uniqueCompanies)} farklı şirkete daha yorum yapın";
                        break;

                    default:
                        badgeProgress.ProgressPercentage = 0;
                        badgeProgress.CurrentStatus = "Henüz başlanmadı";
                        badgeProgress.NextRequirement = badge.Criteria;
                        break;
                }
            }
            else
            {
                badgeProgress.ProgressPercentage = 100;
                badgeProgress.CurrentStatus = "Kazanıldı!";
                badgeProgress.NextRequirement = "";
            }

            progress[badge.Id] = badgeProgress;
        }

        return progress;
    }

    public async Task<int> CalculateUserBadgePointsAsync(string userId)
    {
        var userBadges = await _unitOfWork.Repository<UserBadge>().GetAsync(ub => ub.UserId == userId);
        var badgeIds = userBadges.Select(ub => ub.BadgeId).ToList();

        if (!badgeIds.Any())
            return 0;

        var badges = await _unitOfWork.Repository<Badge>().GetAsync(b => badgeIds.Contains(b.Id));

        // Her rozet türü için farklı puan
        var totalPoints = 0;
        foreach (var badge in badges)
        {
            totalPoints += badge.Points; // Badge entity'sindeki Points değeri
        }

        return totalPoints;
    }
}
