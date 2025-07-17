namespace RateTheWork.Domain.ValueObjects.Company;

/// <summary>
/// Şirket yorum istatistiklerini temsil eden value object
/// </summary>
public sealed class CompanyReviewStatistics
{
    private CompanyReviewStatistics
    (
        decimal averageRating
        , int totalReviews
        , int verifiedReviews
        , Dictionary<int, int> ratingDistribution
        , Dictionary<string, decimal> categoryAverages
    )
    {
        AverageRating = Math.Round(averageRating, 2);
        TotalReviews = totalReviews;
        VerifiedReviews = verifiedReviews;
        VerifiedPercentage = totalReviews > 0
            ? Math.Round((decimal)verifiedReviews / totalReviews * 100, 2)
            : 0;
        RatingDistribution = ratingDistribution ?? new Dictionary<int, int>();
        CategoryAverages = categoryAverages ?? new Dictionary<string, decimal>();
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Ortalama puan
    /// </summary>
    public decimal AverageRating { get; }

    /// <summary>
    /// Toplam yorum sayısı
    /// </summary>
    public int TotalReviews { get; }

    /// <summary>
    /// Doğrulanmış yorum sayısı
    /// </summary>
    public int VerifiedReviews { get; }

    /// <summary>
    /// Doğrulanmış yorum yüzdesi
    /// </summary>
    public decimal VerifiedPercentage { get; }

    /// <summary>
    /// Puan dağılımı (1-5 arası her puan için sayı)
    /// </summary>
    public Dictionary<int, int> RatingDistribution { get; }

    /// <summary>
    /// Kategori bazlı ortalama puanlar
    /// </summary>
    public Dictionary<string, decimal> CategoryAverages { get; }

    /// <summary>
    /// Son güncelleme tarihi
    /// </summary>
    public DateTime LastUpdated { get; }

    /// <summary>
    /// Yeni istatistik oluşturur
    /// </summary>
    public static CompanyReviewStatistics Create
    (
        decimal averageRating
        , int totalReviews
        , int verifiedReviews
        , Dictionary<int, int> ratingDistribution
        , Dictionary<string, decimal> categoryAverages
    )
    {
        // Validasyonlar
        if (averageRating < 0 || averageRating > 5)
            throw new ArgumentOutOfRangeException(nameof(averageRating), "Ortalama puan 0-5 arasında olmalıdır.");

        if (totalReviews < 0)
            throw new ArgumentOutOfRangeException(nameof(totalReviews), "Toplam yorum sayısı negatif olamaz.");

        if (verifiedReviews < 0)
            throw new ArgumentOutOfRangeException(nameof(verifiedReviews), "Doğrulanmış yorum sayısı negatif olamaz.");

        if (verifiedReviews > totalReviews)
            throw new ArgumentException("Doğrulanmış yorum sayısı toplam yorum sayısından fazla olamaz.");

        // Rating distribution kontrolü
        if (ratingDistribution != null)
        {
            foreach (var kvp in ratingDistribution)
            {
                if (kvp.Key < 1 || kvp.Key > 5)
                    throw new ArgumentException($"Geçersiz puan değeri: {kvp.Key}. Puanlar 1-5 arasında olmalıdır.");

                if (kvp.Value < 0)
                    throw new ArgumentException($"Puan sayısı negatif olamaz: {kvp.Key} puanı için {kvp.Value}");
            }
        }

        return new CompanyReviewStatistics(
            averageRating,
            totalReviews,
            verifiedReviews,
            ratingDistribution,
            categoryAverages
        );
    }

    /// <summary>
    /// Boş istatistik oluşturur (hiç yorum olmadığında)
    /// </summary>
    public static CompanyReviewStatistics CreateEmpty()
    {
        return new CompanyReviewStatistics(
            averageRating: 0,
            totalReviews: 0,
            verifiedReviews: 0,
            ratingDistribution: new Dictionary<int, int>
            {
                { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
            },
            categoryAverages: new Dictionary<string, decimal>()
        );
    }

    /// <summary>
    /// Belirli bir puan için yorum sayısını döndürür
    /// </summary>
    public int GetReviewCountForRating(int rating)
    {
        return RatingDistribution.TryGetValue(rating, out var count) ? count : 0;
    }

    /// <summary>
    /// Belirli bir kategori için ortalama puanı döndürür
    /// </summary>
    public decimal GetCategoryAverage(string category)
    {
        return CategoryAverages.TryGetValue(category, out var average) ? average : 0;
    }

    /// <summary>
    /// En yüksek puan alan kategoriyi döndürür
    /// </summary>
    public (string Category, decimal Average)? GetHighestRatedCategory()
    {
        if (!CategoryAverages.Any())
            return null;

        var highest = CategoryAverages.OrderByDescending(x => x.Value).First();
        return (highest.Key, highest.Value);
    }

    /// <summary>
    /// En düşük puan alan kategoriyi döndürür
    /// </summary>
    public (string Category, decimal Average)? GetLowestRatedCategory()
    {
        if (!CategoryAverages.Any())
            return null;

        var lowest = CategoryAverages.OrderBy(x => x.Value).First();
        return (lowest.Key, lowest.Value);
    }

    // Value Object equality
    public override bool Equals(object? obj)
    {
        if (obj is not CompanyReviewStatistics other)
            return false;

        return AverageRating == other.AverageRating &&
               TotalReviews == other.TotalReviews &&
               VerifiedReviews == other.VerifiedReviews &&
               RatingDistribution.SequenceEqual(other.RatingDistribution) &&
               CategoryAverages.SequenceEqual(other.CategoryAverages);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + AverageRating.GetHashCode();
            hash = hash * 23 + TotalReviews.GetHashCode();
            hash = hash * 23 + VerifiedReviews.GetHashCode();

            foreach (var item in RatingDistribution)
            {
                hash = hash * 23 + item.Key.GetHashCode();
                hash = hash * 23 + item.Value.GetHashCode();
            }

            foreach (var item in CategoryAverages)
            {
                hash = hash * 23 + item.Key.GetHashCode();
                hash = hash * 23 + item.Value.GetHashCode();
            }

            return hash;
        }
    }
}
