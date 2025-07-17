using System.Text.RegularExpressions;
using RateTheWork.Domain.Constants;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Extensions;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.ValueObjects.Review;

namespace RateTheWork.Domain.Services;

/// <summary>
/// Yorum işlemleri için domain service implementasyonu
/// </summary>
public class ReviewDomainService : IReviewDomainService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewDomainService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CanUserReviewCompanyAsync(string userId, string companyId, string commentType)
    {
        // Kullanıcı kontrolü
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null || user.IsBanned)
            return false;

        // Email doğrulaması yapılmış mı?
        if (!user.IsEmailVerified)
            return false;

        // Şirket kontrolü
        var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
        if (company == null || !company.IsApproved)
            return false;

        // Kullanıcı bu şirkette çalışıyor ve kendi şirketini mi değerlendiriyor?
        if (!string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(company.Email))
        {
            var userDomain = user.Email.Split('@').LastOrDefault()?.ToLowerInvariant();
            if (userDomain == company.Email.ToLowerInvariant())
            {
                // Çalışanlar sadece belirli kategorilerde yorum yapabilir
                var allowedTypesForEmployees = new[] { "Çalışma Ortamı", "Kariyer Gelişimi", "Yan Haklar" };
                if (!allowedTypesForEmployees.Contains(commentType))
                    return false;
            }
        }

        // Aynı tip yorum için cooldown kontrolü
        var lastReview = await _unitOfWork.Repository<Review>()
            .GetFirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.CompanyId == companyId &&
                r.CommentType.ToString() == commentType &&
                r.IsActive);

        if (lastReview != null)
        {
            var daysSinceLastReview = (DateTime.UtcNow - lastReview.CreatedAt).TotalDays;
            return daysSinceLastReview >= DomainConstants.Review.ReviewCooldownDays; // 365 gün
        }

        return true;
    }

    public decimal CalculateHelpfulnessScore(int upvotes, int downvotes, bool isVerified)
    {
        var totalVotes = upvotes + downvotes;
        if (totalVotes == 0)
            return 0;

        // Wilson Score Interval hesaplama (daha doğru sıralama için)
        var positiveRatio = (decimal)upvotes / totalVotes;
        var z = 1.96m; // %95 güven aralığı için z-score
        var n = totalVotes;

        var wilsonScore = (positiveRatio + (z * z) / (2 * n) -
                           z * (decimal)Math.Sqrt((double)(positiveRatio * (1 - positiveRatio) / n +
                                                           (z * z) / (4 * n * n)))) /
                          (1 + (z * z) / n);

        // Doğrulanmış yorumlar için bonus
        if (isVerified)
            wilsonScore *= 1.2m;

        // Minimum oy sayısı penaltısı (az oy alan yorumlar düşük skor alır)
        if (totalVotes < 10)
            wilsonScore *= (decimal)totalVotes / 10;

        return Math.Round(wilsonScore * 100, 2); // 0-100 arası skor
    }

    public async Task UpdateCompanyRatingAsync(string companyId)
    {
        var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
        if (company == null)
            throw new EntityNotFoundException(nameof(Company), companyId);

        var reviews = await _unitOfWork.Repository<Review>().GetReviewsByCompanyAsync(companyId, 1, int.MaxValue);
        var activeReviews = reviews.Where(r => r.IsActive).ToList();

        if (!activeReviews.Any())
        {
            company.UpdateReviewStatistics(0, 0);
            await _unitOfWork.Repository<Company>().UpdateAsync(company);
            return;
        }

        // Ağırlıklı ortalama hesaplama
        var weightedSum = 0m;
        var totalWeight = 0m;

        foreach (var review in activeReviews)
        {
            // Doğrulanmış yorumlar daha yüksek ağırlık
            var weight = review.IsDocumentVerified ? 2.0m : 1.0m;

            // Yardımcılık skoru yüksek olanlar daha yüksek ağırlık
            var helpfulnessMultiplier = 1 + (review.HelpfulnessScore / 100) * 0.5m;
            weight *= helpfulnessMultiplier;

            weightedSum += review.OverallRating * weight;
            totalWeight += weight;
        }

        var weightedAverage = totalWeight > 0 ? weightedSum / totalWeight : 0;
        company.UpdateReviewStatistics(Math.Round(weightedAverage, 2), activeReviews.Count);

        await _unitOfWork.Repository<Company>().UpdateAsync(company);
    }

    public async Task<bool> IsSpamReviewAsync(string userId, string commentText)
    {
        // Son 24 saatte çok fazla yorum kontrolü
        var recentReviews = await _unitOfWork.Repository<Review>()
            .GetAsync(r => r.UserId == userId && r.CreatedAt > DateTime.UtcNow.AddDays(-1));

        if (recentReviews.Count > 3)
            return true;

        // Aynı metni içeren yorum kontrolü
        var normalizedText = NormalizeText(commentText);
        var similarReviews = await _unitOfWork.Repository<Review>()
            .GetAsync(r => r.UserId == userId && r.CreatedAt > DateTime.UtcNow.AddDays(-30));

        foreach (var review in similarReviews)
        {
            var similarity = CalculateTextSimilarity(normalizedText, NormalizeText(review.CommentText));
            if (similarity > 0.8) // %80'den fazla benzerlik
                return true;
        }

        // Spam pattern kontrolü
        var spamPatterns = new[]
        {
            @"(http|https|www\.)", @"(\b\d{10,}\b)", // Uzun sayı dizileri
            @"([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})"
            , // Email
            @"(\+?\d{1,3}[-.\s]?\(?\d{1,4}\)?[-.\s]?\d{1,4}[-.\s]?\d{1,9})" // Telefon
        };

        foreach (var pattern in spamPatterns)
        {
            if (Regex.IsMatch(commentText, pattern))
                return true;
        }

        return false;
    }

    public async Task<ReviewQualityScore> CalculateReviewQualityAsync(string reviewId)
    {
        var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId);
        if (review == null)
            throw new EntityNotFoundException(nameof(Review), reviewId);

        // Uzunluk skoru (50-2000 karakter arası ideal)
        var length = review.CommentText.Length;
        decimal lengthScore;
        if (length >= 200 && length <= 1000)
            lengthScore = 100;
        else if (length >= 100 && length <= 2000)
            lengthScore = 80;
        else if (length >= 50)
            lengthScore = 60;
        else
            lengthScore = 30;

        // Detay skoru (cümle sayısı, kelime çeşitliliği)
        var sentences = review.CommentText.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var words = review.CommentText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var uniqueWords = words.Distinct(StringComparer.OrdinalIgnoreCase).Count();

        var detailScore = Math.Min(100, (sentences.Length * 10) + (uniqueWords * 2));

        // Objektiflik skoru (subjektif kelimeler kontrolü)
        var subjectiveWords = new[] { "berbat", "mükemmel", "rezalet", "harika", "korkunç", "muhteşem" };
        var subjectiveCount = subjectiveWords.Count(word =>
            review.CommentText.Contains(word, StringComparison.OrdinalIgnoreCase));

        var objectivityScore = Math.Max(0, 100 - (subjectiveCount * 20));

        // Helpfulness skoru
        var helpfulnessScore = review.HelpfulnessScore;

        // İyileştirme önerileri
        var suggestions = new List<string>();
        if (lengthScore < 60) suggestions.Add("Daha detaylı açıklama yapılabilir");
        if (detailScore < 50) suggestions.Add("Daha çok örnek verilebilir");
        if (objectivityScore < 70) suggestions.Add("Daha objektif ifadeler kullanılabilir");

        var score = ReviewQualityScore.Create(lengthScore, detailScore, objectivityScore, helpfulnessScore
            , suggestions);

        // İyileştirme önerileri
        if (score.LengthScore < 60)
            score.ImprovementSuggestions.Add("Yorumunuzu biraz daha detaylandırabilirsiniz.");

        if (score.DetailScore < 60)
            score.ImprovementSuggestions.Add("Daha fazla örnek ve detay ekleyebilirsiniz.");

        if (score.ObjectivityScore < 60)
            score.ImprovementSuggestions.Add("Daha objektif bir dil kullanmayı deneyebilirsiniz.");

        return score;
    }

    public async Task<List<string?>> FindSimilarReviewsAsync(string userId, string commentText)
    {
        var normalizedNewText = NormalizeText(commentText);
        var userReviews = await _unitOfWork.Repository<Review>().GetReviewsByUserAsync(userId, 1, int.MaxValue);
        var similarReviewIds = new List<string?>();

        foreach (var review in userReviews.Where(r => r.IsActive))
        {
            var similarity = CalculateTextSimilarity(normalizedNewText, NormalizeText(review.CommentText));
            if (similarity > 0.7) // %70'ten fazla benzerlik
            {
                similarReviewIds.Add(review.Id);
            }
        }

        return similarReviewIds;
    }

    public async Task<ReviewTrends> AnalyzeReviewTrendsAsync(string companyId, DateTime startDate, DateTime endDate)
    {
        var reviews = await _unitOfWork.Repository<Review>()
            .GetAsync(r => r.CompanyId == companyId &&
                           r.CreatedAt >= startDate &&
                           r.CreatedAt <= endDate &&
                           r.IsActive);

        // Kategori ortalamaları
        var categoryAverages = reviews.GroupBy(r => r.CommentType)
            .ToDictionary(g => g.Key.ToString(), g => Math.Round(g.Average(r => r.OverallRating), 2));

        // Tarih bazlı yorum sayıları
        var reviewCountByDate = reviews.GroupBy(r => r.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        // En çok bahsedilen olumlu/olumsuz konular (basit kelime analizi)
        var positiveKeywords = new[] { "güzel", "iyi", "harika", "mükemmel", "başarılı", "kaliteli" };
        var negativeKeywords = new[] { "kötü", "berbat", "yetersiz", "zayıf", "sorunlu", "eksik" };

        var positiveMatches = new List<string>();
        var negativeMatches = new List<string>();

        foreach (var review in reviews)
        {
            var lowerText = review.CommentText.ToLowerInvariant();

            foreach (var keyword in positiveKeywords)
            {
                if (lowerText.Contains(keyword) && !positiveMatches.Contains(keyword))
                {
                    positiveMatches.Add(keyword);
                }
            }

            foreach (var keyword in negativeKeywords)
            {
                if (lowerText.Contains(keyword) && !negativeMatches.Contains(keyword))
                {
                    negativeMatches.Add(keyword);
                }
            }
        }

        // Sentiment trend (basit hesaplama)
        var avgRatingByMonth = reviews
            .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
            .Select(g => new { Date = g.Key, Avg = g.Average(r => r.OverallRating) })
            .OrderBy(x => x.Date.Year).ThenBy(x => x.Date.Month)
            .ToList();

        decimal sentimentTrend = 0;
        if (avgRatingByMonth.Count >= 2)
        {
            var firstMonthAvg = avgRatingByMonth.First().Avg;
            var lastMonthAvg = avgRatingByMonth.Last().Avg;
            sentimentTrend = (lastMonthAvg - firstMonthAvg) / firstMonthAvg;
        }

        return ReviewTrends.Create(
            categoryAverages,
            reviewCountByDate,
            positiveMatches,
            negativeMatches,
            sentimentTrend);
    }

    // Helper metodlar
    private string NormalizeText(string text)
    {
        // Küçük harfe çevir, noktalama işaretlerini kaldır
        var normalized = text.ToLowerInvariant();
        normalized = Regex.Replace(normalized, @"[^\w\s]", " ");
        normalized = Regex.Replace(normalized, @"\s+", " ");
        return normalized.Trim();
    }

    private double CalculateTextSimilarity(string text1, string text2)
    {
        // Basit Jaccard similarity hesaplama
        var words1 = text1.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var words2 = text2.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        if (!words1.Any() || !words2.Any())
            return 0;

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return (double)intersection / union;
    }
}
