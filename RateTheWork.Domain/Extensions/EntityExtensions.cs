using RateTheWork.Domain.Common;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Report;
using RateTheWork.Domain.Enums.User;
using RateTheWork.Domain.Enums.VerificationRequest;

namespace RateTheWork.Domain.Extensions;

/// <summary>
/// Entity'ler için extension metodları
/// </summary>
public static class EntityExtensions
{
    // User Extensions
    /// <summary>
    /// Kullanıcının görüntü adını döndürür
    /// </summary>
    public static string GetDisplayName(this User user)
    {
        return user.AnonymousUsername;
    }

    /// <summary>
    /// Kullanıcının anonim görüntülenecek adını döndürür
    /// </summary>
    public static string GetAnonymousDisplayName(this User user, AnonymityLevel level)
    {
        var username = user.AnonymousUsername;
        if (string.IsNullOrEmpty(username) || username.Length < 3)
            return "Anonim Kullanıcı";

        return level switch
        {
            AnonymityLevel.Full => "Anonim Kullanıcı", AnonymityLevel.High => $"{username.Substring(0, 1)}***"
            , AnonymityLevel.Medium => username, AnonymityLevel.Low => username
            , AnonymityLevel.Partial => $"{username.Substring(0, Math.Min(3, username.Length))}***"
            , AnonymityLevel.VerifiedAnonymous => "Doğrulanmış Anonim", AnonymityLevel.Public => username
            , _ => "Anonim Kullanıcı"
        };
    }

    /// <summary>
    /// Kullanıcının hesap yaşını gün olarak döndürür
    /// </summary>
    public static int GetAccountAgeInDays(this User user)
    {
        return (DateTime.UtcNow - user.CreatedAt).Days;
    }

    /// <summary>
    /// Kullanıcının doğrulanmış olup olmadığını kontrol eder
    /// </summary>
    public static bool IsFullyVerified(this User user)
    {
        return user.IsEmailVerified && user.IsActive && !user.IsBanned;
    }

    // Review Extensions
    /// <summary>
    /// Yorumun net beğeni sayısını hesaplar
    /// </summary>
    public static int GetNetVotes(this Review review)
    {
        return review.Upvotes - review.Downvotes;
    }

    /// <summary>
    /// Yorumun popülerlik skorunu hesaplar
    /// </summary>
    public static double GetPopularityScore(this Review review)
    {
        var totalVotes = review.Upvotes + review.Downvotes;
        if (totalVotes == 0) return 0;

        var positiveRatio = (double)review.Upvotes / totalVotes;
        var confidence = 1 - (1.0 / (totalVotes + 1));

        return positiveRatio * confidence * 100;
    }

    /// <summary>
    /// Yorumun yaşını gün olarak döndürür
    /// </summary>
    public static int GetAgeInDays(this Review review)
    {
        return (DateTime.UtcNow - review.CreatedAt).Days;
    }

    /// <summary>
    /// Yorumun güvenilirlik rozetini döndürür
    /// </summary>
    public static string GetReliabilityBadge(this Review review)
    {
        if (review.IsDocumentVerified)
            return "Belgeli Yorum";

        if (review.HelpfulnessScore >= 80)
            return "Faydalı Yorum";

        return string.Empty;
    }

    // Company Extensions
    /// <summary>
    /// Şirketin ortalama puanını hesaplar
    /// </summary>
    public static decimal GetAverageRating(this Company company)
    {
        if (company.ReviewStatistics.TotalReviews == 0)
            return 0;

        return company.ReviewStatistics.AverageRating;
    }

    /// <summary>
    /// Şirketin çalışan sayısı kategorisini döndürür
    /// </summary>
    public static string GetEmployeeSizeCategory(this Company company)
    {
        if (company.EmployeeCount.HasValue)
        {
            return company.EmployeeCount.Value switch
            {
                <= 10 => "Mikro İşletme", <= 50 => "Küçük İşletme", <= 250 => "Orta Ölçekli İşletme"
                , <= 1000 => "Büyük İşletme", _ => "Kurumsal İşletme"
            };
        }

        // EmployeeCountRange kullan
        if (!string.IsNullOrEmpty(company.EmployeeCountRange))
        {
            var range = company.EmployeeCountRange;
            if (range.Contains("1-10") || range.Contains("0-10"))
                return "Mikro İşletme";
            else if (range.Contains("11-50") || range.Contains("10-50"))
                return "Küçük İşletme";
            else if (range.Contains("51-250") || range.Contains("50-250"))
                return "Orta Ölçekli İşletme";
            else if (range.Contains("251-1000") || range.Contains("250-1000"))
                return "Büyük İşletme";
            else
                return "Kurumsal İşletme";
        }

        return "Bilinmiyor";
    }

    /// <summary>
    /// Şirketin yaşını yıl olarak döndürür
    /// </summary>
    public static int GetCompanyAgeInYears(this Company company)
    {
        return DateTime.UtcNow.Year - company.EstablishedYear;
    }

    /// <summary>
    /// Şirketin güvenilirlik seviyesini döndürür
    /// </summary>
    public static string GetTrustLevel(this Company company)
    {
        // TrustScore olmadığı için ortalama puan ve doğrulanmış yorum oranına göre hesaplama
        var avgRating = company.ReviewStatistics.AverageRating;
        var verifiedPercentage = company.ReviewStatistics.VerifiedPercentage;

        var trustScore = (avgRating * 20) + (verifiedPercentage * 0.3m);

        return trustScore switch
        {
            >= 90 => "Çok Yüksek", >= 75 => "Yüksek", >= 60 => "Orta", >= 40 => "Düşük", _ => "Çok Düşük"
        };
    }

    // Badge Extensions
    /// <summary>
    /// Rozetin aktif olup olmadığını kontrol eder
    /// </summary>
    public static bool IsBadgeActive(this Badge badge)
    {
        return badge.IsActive &&
               (!badge.AvailableFrom.HasValue || badge.AvailableFrom.Value <= DateTime.UtcNow) &&
               (!badge.AvailableUntil.HasValue || badge.AvailableUntil.Value >= DateTime.UtcNow);
    }

    /// <summary>
    /// Rozetin kalan geçerlilik süresini gün olarak döndürür
    /// </summary>
    public static int? GetRemainingDays(this Badge badge)
    {
        if (!badge.AvailableUntil.HasValue)
            return null;

        var remainingDays = (badge.AvailableUntil.Value - DateTime.UtcNow).Days;
        return remainingDays > 0 ? remainingDays : 0;
    }

    // TODO: BadgeProgress entity oluşturulduktan sonra aşağıdaki extension'lar eklenecek
    // BadgeProgress Extensions
    // GetProgressPercentage() - İlerleme yüzdesini hesaplar
    // GetRemainingValue() - Rozet kazanmaya ne kadar kaldığını hesaplar

    // Report Extensions
    /// <summary>
    /// Raporun kritik olup olmadığını kontrol eder
    /// </summary>
    public static bool IsCritical(this Report report)
    {
        return report.Priority >= 4 || // Yüksek öncelik (1-5 skalasında)
               report.RequiresUrgentAction ||
               report.Status == ReportStatus.UnderReview;
    }

    /// <summary>
    /// Raporun yaşını saat olarak döndürür
    /// </summary>
    public static int GetAgeInHours(this Report report)
    {
        return (int)(DateTime.UtcNow - report.CreatedAt).TotalHours;
    }

    // VerificationRequest Extensions
    /// <summary>
    /// Doğrulama talebinin süresinin dolup dolmadığını kontrol eder
    /// </summary>
    public static bool IsExpired(this VerificationRequest request, int expirationDays = 30)
    {
        return request.Status == VerificationRequestStatus.Pending &&
               (DateTime.UtcNow - request.CreatedAt).Days > expirationDays;
    }

    /// <summary>
    /// Doğrulama talebinin işlem görmesini bekleyen gün sayısını döndürür
    /// </summary>
    public static int GetPendingDays(this VerificationRequest request)
    {
        if (request.Status != VerificationRequestStatus.Pending)
            return 0;

        return (DateTime.UtcNow - request.CreatedAt).Days;
    }

    // Common Extensions for BaseEntity
    /// <summary>
    /// Entity'nin yaşını döndürür
    /// </summary>
    public static TimeSpan GetAge<T>(this T entity) where T : BaseEntity
    {
        return DateTime.UtcNow - entity.CreatedAt;
    }

    /// <summary>
    /// Entity'nin bugün oluşturulup oluşturulmadığını kontrol eder
    /// </summary>
    public static bool IsCreatedToday<T>(this T entity) where T : BaseEntity
    {
        return entity.CreatedAt.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Entity'nin bu hafta oluşturulup oluşturulmadığını kontrol eder
    /// </summary>
    public static bool IsCreatedThisWeek<T>(this T entity) where T : BaseEntity
    {
        var startOfWeek = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        return entity.CreatedAt >= startOfWeek;
    }

    /// <summary>
    /// Entity'nin bu ay oluşturulup oluşturulmadığını kontrol eder
    /// </summary>
    public static bool IsCreatedThisMonth<T>(this T entity) where T : BaseEntity
    {
        return entity.CreatedAt.Year == DateTime.UtcNow.Year &&
               entity.CreatedAt.Month == DateTime.UtcNow.Month;
    }
}
