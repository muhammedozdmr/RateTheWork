using System.Text.RegularExpressions;
using RateTheWork.Domain.Constants;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.ValueObjects;

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
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.IsBanned)
            return false;

        // Email doğrulaması yapılmış mı?
        if (!user.IsEmailVerified)
            return false;

        // Şirket kontrolü
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
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
        var lastReview = await _unitOfWork.Reviews
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
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new EntityNotFoundException(nameof(Company), companyId);

        var reviews = await _unitOfWork.Reviews.GetReviewsByCompanyAsync(companyId, 1, int.MaxValue);
        var activeReviews = reviews.Where(r => r.IsActive).ToList();

        if (!activeReviews.Any())
        {
            company.UpdateReviewStatistics(0, 0);
            await _unitOfWork.Companies.UpdateAsync(company);
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

        await _unitOfWork.Companies.UpdateAsync(company);
    }

    public async Task<bool> IsSpamReviewAsync(string userId, string commentText)
    {
        // Son 24 saatte çok fazla yorum kontrolü
        var recentReviews = await _unitOfWork.Reviews
            .GetAsync(r => r.UserId == userId && r.CreatedAt > DateTime.UtcNow.AddDays(-1));

        if (recentReviews.Count > 3)
            return true;

        // Aynı metni içeren yorum kontrolü
        var normalizedText = NormalizeText(commentText);
        var similarReviews = await _unitOfWork.Reviews
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
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review == null)
            throw new EntityNotFoundException(nameof(Review), reviewId);

        var score = new ReviewQualityScore();

        // Uzunluk skoru (50-2000 karakter arası ideal)
        var length = review.CommentText.Length;
        if (length >= 200 && length <= 1000)
            score.LengthScore = 100;
        else if (length >= 100 && length <= 2000)
            score.LengthScore = 80;
        else if (length >= 50)
            score.LengthScore = 60;
        else
            score.LengthScore = 30;

        // Detay skoru (cümle sayısı, kelime çeşitliliği)
        var sentences = review.CommentText.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var words = review.CommentText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var uniqueWords = words.Distinct(StringComparer.OrdinalIgnoreCase).Count();

        score.DetailScore = Math.Min(100, (sentences.Length * 10) + (uniqueWords * 2));

        // Objektiflik skoru (subjektif kelimeler kontrolü)
        var subjectiveWords = new[] { "berbat", "mükemmel", "rezalet", "harika", "korkunç", "muhteşem" };
        var subjectiveCount = subjectiveWords.Count(word =>
            review.CommentText.Contains(word, StringComparison.OrdinalIgnoreCase));

        score.ObjectivityScore = Math.Max(0, 100 - (subjectiveCount * 20));

        // Helpfulness skoru
        score.HelpfulnessScore = review.HelpfulnessScore;

        // Genel skor
        score.OverallScore = (score.LengthScore * 0.2m +
                              score.DetailScore * 0.3m +
                              score.ObjectivityScore * 0.2m +
                              score.HelpfulnessScore * 0.3m);

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
        var userReviews = await _unitOfWork.Reviews.GetReviewsByUserAsync(userId, 1, int.MaxValue);
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
        var reviews = await _unitOfWork.Reviews
            .GetAsync(r => r.CompanyId == companyId &&
                           r.CreatedAt >= startDate &&
                           r.CreatedAt <= endDate &&
                           r.IsActive);

        var trends = new ReviewTrends();

        // Kategori ortalamaları
        var categoryGroups = reviews.GroupBy(r => r.CommentType);
        foreach (var group in categoryGroups)
        {
            var average = group.Average(r => r.OverallRating);
            trends.CategoryAverages[group.Key.ToString()] = Math.Round(average, 2);
        }

        // Tarih bazlı yorum sayıları
        var dateGroups = reviews.GroupBy(r => r.CreatedAt.Date);
        foreach (var group in dateGroups)
        {
            trends.ReviewCountByDate[group.Key] = group.Count();
        }

        // En çok bahsedilen olumlu/olumsuz konular (basit kelime analizi)
        var positiveKeywords = new[] { "güzel", "iyi", "harika", "mükemmel", "başarılı", "kaliteli" };
        var negativeKeywords = new[] { "kötü", "berbat", "yetersiz", "zayıf", "sorunlu", "eksik" };

        foreach (var review in reviews)
        {
            var lowerText = review.CommentText.ToLowerInvariant();

            foreach (var keyword in positiveKeywords)
            {
                if (lowerText.Contains(keyword))
                {
                    if (!trends.MostMentionedPositives.Contains(keyword))
                        trends.MostMentionedPositives.Add(keyword);
                }
            }

            foreach (var keyword in negativeKeywords)
            {
                if (lowerText.Contains(keyword))
                {
                    if (!trends.MostMentionedNegatives.Contains(keyword))
                        trends.MostMentionedNegatives.Add(keyword);
                }
            }
        }

        // Sentiment trend (basit hesaplama)
        var avgRatingByMonth = reviews
            .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
            .Select(g => new { Date = g.Key, Avg = g.Average(r => r.OverallRating) })
            .OrderBy(x => x.Date.Year).ThenBy(x => x.Date.Month)
            .ToList();

        if (avgRatingByMonth.Count >= 2)
        {
            var firstMonthAvg = avgRatingByMonth.First().Avg;
            var lastMonthAvg = avgRatingByMonth.Last().Avg;
            trends.SentimentTrend = (lastMonthAvg - firstMonthAvg) / firstMonthAvg;
        }

        return trends;
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
