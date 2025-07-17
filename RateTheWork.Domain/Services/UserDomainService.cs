using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.User;
using RateTheWork.Domain.Enums.VerificationRequest;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Extensions;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.ValueObjects.User;

namespace RateTheWork.Domain.Services;

/// <summary>
/// Kullanıcı işlemleri için domain service implementasyonu
/// </summary>
public class UserDomainService : IUserDomainService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsUserActiveAsync(string userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
            return false;

        return user.IsActive && !user.IsBanned && user.IsEmailVerified;
    }

    public async Task<bool> CanUserCreateReviewAsync(string userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
            return false;

        // Temel kontroller
        if (!user.IsActive || user.IsBanned || !user.IsEmailVerified)
            return false;

        // Yeni kullanıcı için bekleme süresi (24 saat)
        var hoursSinceRegistration = (DateTime.UtcNow - user.CreatedAt).TotalHours;
        if (hoursSinceRegistration < 24)
            return false;

        // Günlük yorum limiti kontrolü
        var todayReviewCount = await _unitOfWork.Repository<Review>()
            .CountAsync(r => r.UserId == userId &&
                             r.CreatedAt.Date == DateTime.UtcNow.Date &&
                             r.IsActive);

        return todayReviewCount < 5; // Günde maksimum 5 yorum
    }

    public async Task<decimal> CalculateUserReliabilityScoreAsync(string userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
            return 0;

        var reviews = await _unitOfWork.Repository<Review>()
            .GetReviewsByUserAsync(userId, 1, int.MaxValue);

        var activeReviews = reviews.Where(r => r.IsActive).ToList();

        if (!activeReviews.Any())
            return 50; // Varsayılan skor

        decimal score = 0;

        // Email doğrulaması
        if (user.IsEmailVerified)
            score += 20;

        // Hesap yaşı
        var accountAge = (DateTime.UtcNow - user.CreatedAt).TotalDays;
        if (accountAge >= 365)
            score += 20;
        else if (accountAge >= 180)
            score += 15;
        else if (accountAge >= 90)
            score += 10;
        else if (accountAge >= 30)
            score += 5;

        // Yorum sayısı ve kalitesi
        var verifiedReviews = activeReviews.Count(r => r.IsDocumentVerified);
        var verificationRate = activeReviews.Count > 0 ? (decimal)verifiedReviews / activeReviews.Count : 0;

        score += verificationRate * 25;

        // Yorum beğeni oranı
        var totalUpvotes = activeReviews.Sum(r => r.Upvotes);
        var totalDownvotes = activeReviews.Sum(r => r.Downvotes);
        var totalVotes = totalUpvotes + totalDownvotes;

        if (totalVotes > 0)
        {
            var approvalRate = (decimal)totalUpvotes / totalVotes;
            score += approvalRate * 20;
        }

        // Tutarlılık skoru (ortalama puan sapması)
        if (activeReviews.Count >= 5)
        {
            var averageRating = activeReviews.Average(r => r.OverallRating);
            var variance = activeReviews.Average(r => Math.Pow((double)(r.OverallRating - averageRating), 2));
            var consistency = Math.Max(0, 1 - (variance / 5)); // 0-1 arası
            score += (decimal)consistency * 15;
        }

        return Math.Min(score, 100);
    }

    public async Task<string> DetermineAnonymityLevelAsync(string userId, string companyId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);

        if (user == null || company == null)
            return AnonymityLevel.Full.ToString();

        // Kullanıcının şirkette çalışıp çalışmadığını kontrol et
        var isEmployee = await IsUserEmployeeAsync(userId, companyId);

        if (isEmployee)
        {
            // Çalışan için yüksek anonimlik
            return AnonymityLevel.High.ToString();
        }

        // Kullanıcı güvenilirlik skoru
        var reliabilityScore = await CalculateUserReliabilityScoreAsync(userId);

        if (reliabilityScore >= 80)
            return AnonymityLevel.Low.ToString();
        else if (reliabilityScore >= 60)
            return AnonymityLevel.Medium.ToString();
        else
            return AnonymityLevel.High.ToString();
    }

    public async Task<bool> VerifyEmploymentAsync(string userId, string companyId, string verificationMethod)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);

        if (user == null || company == null)
            return false;

        switch (verificationMethod.ToLowerInvariant())
        {
            case "email":
                return await VerifyEmploymentByEmailAsync(user, company);

            case "document":
                return await VerifyEmploymentByDocumentAsync(userId, companyId);

            default:
                return false;
        }
    }

    public async Task<UserActivitySummary> GetUserActivitySummaryAsync(string userId, DateTime? startDate = null)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException(nameof(User), userId);

        var filterDate = startDate ?? DateTime.UtcNow.AddDays(-30);

        var reviews = await _unitOfWork.Repository<Review>()
            .GetAsync(r => r.UserId == userId && r.CreatedAt >= filterDate && r.IsActive);

        var votes = await _unitOfWork.Repository<ReviewVote>()
            .GetAsync(v => v.UserId == userId && v.CreatedAt >= filterDate);

        var totalReviews = reviews.Count;
        var verifiedReviews = reviews.Count(r => r.IsDocumentVerified);
        var helpfulVotes = votes.Count(v => v.IsUpvote);
        var unhelpfulVotes = votes.Count(v => !v.IsUpvote);
        var averageRating = reviews.Any() ? reviews.Average(r => r.OverallRating) : 0;

        var mostReviewedSectors = reviews
            .GroupBy(r => r.CommentType)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key.ToString())
            .ToList();

        var activityByMonth = reviews
            .GroupBy(r => new DateTime(r.CreatedAt.Year, r.CreatedAt.Month, 1))
            .ToDictionary(g => g.Key, g => g.Count());

        var consecutiveActiveDays = CalculateConsecutiveActiveDays(reviews);

        var summary = UserActivitySummary.Create(
            totalReviews, verifiedReviews, helpfulVotes, unhelpfulVotes,
            averageRating, mostReviewedSectors, activityByMonth, consecutiveActiveDays);

        return summary;
    }

    public async Task<UserPreferences> AnalyzeUserPreferencesAsync(string userId)
    {
        var reviews = await _unitOfWork.Repository<Review>()
            .GetReviewsByUserAsync(userId, 1, int.MaxValue);

        var activeReviews = reviews.Where(r => r.IsActive).ToList();

        var preferences = new UserPreferences
        {
            PreferredCategories = new List<string>(), PreferredCompanySizes = new List<string>()
            , PreferredSectors = new List<string>()
        };

        if (!activeReviews.Any())
            return preferences;

        // En çok yorum yapılan kategoriler
        var categoryPreferences = activeReviews
            .GroupBy(r => r.CommentType)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key.ToString())
            .ToList();

        preferences.PreferredCategories = categoryPreferences;

        // Şirket sektör tercihleri
        var companyIds = activeReviews.Select(r => r.CompanyId).Distinct().ToList();
        var companies = await _unitOfWork.Repository<Company>()
            .GetAsync(c => companyIds.Contains(c.Id));

        var sectorPreferences = companies
            .GroupBy(c => c.Sector)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key.ToString())
            .ToList();

        preferences.PreferredSectors = sectorPreferences;

        return preferences;
    }

    public async Task<List<Company>> GetRecommendedCompaniesAsync(string userId, int maxResults = 10)
    {
        var userPreferences = await AnalyzeUserPreferencesAsync(userId);
        var userReviews = await _unitOfWork.Repository<Review>()
            .GetReviewsByUserAsync(userId, 1, int.MaxValue);

        var reviewedCompanyIds = userReviews.Select(r => r.CompanyId).ToHashSet();

        // Kullanıcının yorum yapmadığı şirketler
        var availableCompanies = await _unitOfWork.Repository<Company>()
            .GetAsync(c => !reviewedCompanyIds.Contains(c.Id) && c.IsApproved);

        var recommendations = new List<Company>();

        // Tercih edilen sektörlerden şirketler
        if (userPreferences.PreferredSectors.Any())
        {
            var sectorRecommendations = availableCompanies
                .Where(c => userPreferences.PreferredSectors.Contains(c.Sector.ToString()))
                .OrderByDescending(c => c.ReviewStatistics.TotalReviews)
                .Take(maxResults / 2);

            recommendations.AddRange(sectorRecommendations);
        }

        // Popüler şirketler
        if (recommendations.Count < maxResults)
        {
            var popularCompanies = availableCompanies
                .Where(c => !recommendations.Contains(c))
                .OrderByDescending(c => c.ReviewStatistics.TotalReviews)
                .Take(maxResults - recommendations.Count);

            recommendations.AddRange(popularCompanies);
        }

        return recommendations.Take(maxResults).ToList();
    }

    public async Task<UserBehaviorScore> CalculateUserBehaviorScoreAsync(string userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException(nameof(User), userId);

        var reviews = await _unitOfWork.Repository<Review>()
            .GetReviewsByUserAsync(userId, 1, int.MaxValue);

        var activeReviews = reviews.Where(r => r.IsActive).ToList();

        if (!activeReviews.Any())
        {
            return UserBehaviorScore.Create(
                overallScore: 0,
                consistencyScore: 0,
                objectivityScore: 0,
                engagementScore: 0,
                trustworthinessScore: 0,
                activityScore: 0,
                qualityScore: 0,
                positiveBehaviors: new List<string>(),
                improvementAreas: new List<string> { "Hiç yorum yapılmamış" }
            );
        }

        // Aktivite skoru (yorum sayısı ve sıklığı)
        var reviewCount = activeReviews.Count;
        var daysSinceFirstReview = (DateTime.UtcNow - activeReviews.Min(r => r.CreatedAt)).TotalDays;
        var averageReviewsPerMonth = daysSinceFirstReview > 0 ? (reviewCount / daysSinceFirstReview) * 30 : 0;
        var activityScore = Math.Min((decimal)(averageReviewsPerMonth * 20), 100);

        // Kalite skoru (ortalama helpfulness)
        var avgHelpfulness = activeReviews.Average(r => r.HelpfulnessScore);
        var qualityScore = Math.Min(avgHelpfulness, 100);

        // Tutarlılık skoru (puan sapması)
        var consistencyScore = 0m;
        if (activeReviews.Count >= 3)
        {
            var averageRating = activeReviews.Average(r => r.OverallRating);
            var variance = activeReviews.Average(r => Math.Pow((double)(r.OverallRating - averageRating), 2));
            consistencyScore = Math.Max(0, 100 - (decimal)(variance * 20));
        }

        // Etkileşim skoru (aldığı oylar)
        var totalVotes = activeReviews.Sum(r => r.Upvotes + r.Downvotes);
        var avgVotesPerReview = reviewCount > 0 ? (double)totalVotes / reviewCount : 0;
        var engagementScore = Math.Min((decimal)(avgVotesPerReview * 10), 100);

        // Objektiflik skoru (çeşitli şirketlerde yorum yapma)
        var uniqueCompanies = activeReviews.Select(r => r.CompanyId).Distinct().Count();
        var objectivityScore = Math.Min(uniqueCompanies * 10, 100);

        // Güvenilirlik skoru (doğrulanmış yorumlar)
        var verifiedReviews = activeReviews.Count(r => r.IsDocumentVerified);
        var trustworthinessScore = reviewCount > 0 ? (decimal)verifiedReviews / reviewCount * 100 : 0;

        // Genel skor
        var overallScore = (activityScore * 0.2m +
                            qualityScore * 0.3m +
                            consistencyScore * 0.2m +
                            engagementScore * 0.3m);

        // Pozitif davranışlar ve gelişim alanları
        var positiveBehaviors = new List<string>();
        var improvementAreas = new List<string>();

        if (activityScore >= 80) positiveBehaviors.Add("Yüksek aktivite");
        if (qualityScore >= 80) positiveBehaviors.Add("Kaliteli yorumlar");
        if (consistencyScore >= 80) positiveBehaviors.Add("Tutarlı değerlendirmeler");
        if (engagementScore >= 80) positiveBehaviors.Add("Yüksek etkileşim");
        if (trustworthinessScore >= 80) positiveBehaviors.Add("Güvenilir kullanıcı");

        if (activityScore < 50) improvementAreas.Add("Daha fazla yorum yapılabilir");
        if (qualityScore < 50) improvementAreas.Add("Daha faydalı yorumlar yazılabilir");
        if (consistencyScore < 50) improvementAreas.Add("Daha tutarlı puanlama yapılabilir");
        if (engagementScore < 50) improvementAreas.Add("Daha etkileşimli yorumlar yazılabilir");

        return UserBehaviorScore.Create(
            overallScore: overallScore,
            consistencyScore: consistencyScore,
            objectivityScore: objectivityScore,
            engagementScore: engagementScore,
            trustworthinessScore: trustworthinessScore,
            activityScore: activityScore,
            qualityScore: qualityScore,
            positiveBehaviors: positiveBehaviors,
            improvementAreas: improvementAreas
        );
    }

    // Private helper methods
    private async Task<bool> IsUserEmployeeAsync(string userId, string companyId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);

        if (user == null || company == null)
            return false;

        // Email domain kontrolü
        if (!string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(company.Email))
        {
            var userDomain = user.Email.Split('@').LastOrDefault()?.ToLowerInvariant();
            var companyDomain = company.Email.Split('@').LastOrDefault()?.ToLowerInvariant();

            return userDomain == companyDomain;
        }

        return false;
    }

    private async Task<bool> VerifyEmploymentByEmailAsync(User user, Company company)
    {
        if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(company.Email))
            return false;

        var userDomain = user.Email.Split('@').LastOrDefault()?.ToLowerInvariant();
        var companyDomain = company.Email.Split('@').LastOrDefault()?.ToLowerInvariant();

        return userDomain == companyDomain;
    }

    private async Task<bool> VerifyEmploymentByDocumentAsync(string userId, string companyId)
    {
        // Belgeli doğrulama kontrolü
        var verificationRequest = await _unitOfWork.Repository<VerificationRequest>()
            .GetFirstOrDefaultAsync(vr => vr.UserId == userId &&
                                          vr.CompanyId == companyId &&
                                          vr.Status == VerificationRequestStatus.Approved);

        return verificationRequest != null;
    }

    private static int CalculateConsecutiveActiveDays(IEnumerable<Review> reviews)
    {
        var activeDays = reviews
            .Select(r => r.CreatedAt.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (!activeDays.Any()) return 0;

        var consecutiveDays = 1;
        for (int i = 1; i < activeDays.Count; i++)
        {
            if (activeDays[i - 1].AddDays(-1) == activeDays[i])
            {
                consecutiveDays++;
            }
            else
            {
                break;
            }
        }

        return consecutiveDays;
    }
}
