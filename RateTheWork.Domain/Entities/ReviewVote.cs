using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Yorum oyu entity'si - Kullanıcıların yorumlara verdiği upvote/downvote'ları temsil eder.
/// </summary>
public class ReviewVote : BaseEntity
{
    // Vote Types
    public enum VoteType
    {
        Upvote,   // Faydalı
        Downvote  // Faydalı değil
    }

    // Vote Sources
    public enum VoteSource
    {
        Direct,           // Doğrudan yorum sayfasından
        ReviewList,       // Yorum listesinden
        CompanyProfile,   // Şirket profilinden
        UserProfile,      // Kullanıcı profilinden
        SearchResults     // Arama sonuçlarından
    }

    // Properties
    public string? UserId { get; private set; }
    public string? ReviewId { get; private set; }
    public bool IsUpvote { get; private set; }
    public DateTime VotedAt { get; private set; }
    public VoteSource Source { get; private set; }
    public string? IpAddress { get; private set; } // Kötüye kullanım tespiti için
    public string? UserAgent { get; private set; }
    public bool IsVerifiedUser { get; private set; } // Doğrulanmış kullanıcı mı?
    public int UserReputationAtTime { get; private set; } // Oy verildiği andaki kullanıcı itibarı
    public bool WasChanged { get; private set; } // Oy değiştirildi mi?
    public DateTime? LastChangedAt { get; private set; }
    public int ChangeCount { get; private set; } // Kaç kez değiştirildi?
    
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private ReviewVote() : base()
    {
    }

    /// <summary>
    /// EF Core için private constructor
    /// </summary>
    private ReviewVote(string? userId, string? reviewId) : base()
    {
        UserId = userId;
        ReviewId = reviewId;
    }

    /// <summary>
    /// Yeni oy oluşturur
    /// </summary>
    public static ReviewVote Create(
        string userId,
        string reviewId,
        bool isUpvote,
        VoteSource source = VoteSource.Direct,
        bool isVerifiedUser = false,
        int userReputation = 0,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (string.IsNullOrWhiteSpace(reviewId))
            throw new ArgumentNullException(nameof(reviewId));

        var vote = new ReviewVote
        {
            UserId = userId,
            ReviewId = reviewId,
            IsUpvote = isUpvote,
            VotedAt = DateTime.UtcNow,
            Source = source,
            IsVerifiedUser = isVerifiedUser,
            UserReputationAtTime = userReputation,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            WasChanged = false,
            ChangeCount = 0
        };

        // Domain Event
        vote.AddDomainEvent(new ReviewVotedEvent(
            vote.Id,
            userId,
            reviewId,
            isUpvote ? VoteType.Upvote : VoteType.Downvote,
            source
        ));

        return vote;
    }

    /// <summary>
    /// Oyu değiştirir (upvote->downvote veya tersi)
    /// </summary>
    public void ChangeVote()
    {
        var oldVoteType = IsUpvote;
        IsUpvote = !IsUpvote;
        WasChanged = true;
        LastChangedAt = DateTime.UtcNow;
        ChangeCount++;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new ReviewVoteChangedEvent(
            Id,
            UserId,
            ReviewId,
            oldVoteType ? VoteType.Upvote : VoteType.Downvote,
            IsUpvote ? VoteType.Upvote : VoteType.Downvote
        ));
    }

    /// <summary>
    /// Oy değiştirme limitini kontrol et
    /// </summary>
    public bool CanChangeVote(int maxChangesAllowed = 3)
    {
        if (ChangeCount >= maxChangesAllowed)
            return false;

        // Son değişiklikten sonra en az 1 dakika geçmeli
        if (LastChangedAt.HasValue)
        {
            var timeSinceLastChange = DateTime.UtcNow - LastChangedAt.Value;
            if (timeSinceLastChange.TotalMinutes < 1)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Oy ağırlığını hesapla (itibar bazlı)
    /// </summary>
    public decimal CalculateVoteWeight()
    {
        decimal weight = 1.0m;

        // Doğrulanmış kullanıcı bonusu
        if (IsVerifiedUser)
            weight += 0.5m;

        // İtibar bonusu
        if (UserReputationAtTime >= 100)
            weight += 0.3m;
        else if (UserReputationAtTime >= 50)
            weight += 0.2m;
        else if (UserReputationAtTime >= 25)
            weight += 0.1m;

        // Çok fazla değiştirilen oylar daha az değerli
        if (ChangeCount > 2)
            weight *= 0.8m;
        else if (ChangeCount > 0)
            weight *= 0.9m;

        return Math.Max(0.1m, Math.Min(2.0m, weight)); // 0.1 - 2.0 arası
    }

    /// <summary>
    /// Oyun güvenilirlik skorunu hesapla
    /// </summary>
    public int CalculateTrustScore()
    {
        int score = 100; // Başlangıç skoru

        // Çok hızlı oy verme (ilk 10 saniye)
        var timeToVote = (VotedAt - CreatedAt).TotalSeconds;
        if (timeToVote < 10)
            score -= 30;
        else if (timeToVote < 30)
            score -= 10;

        // Çok fazla değiştirme
        score -= ChangeCount * 10;

        // Doğrulanmamış kullanıcı
        if (!IsVerifiedUser)
            score -= 20;

        // Düşük itibar
        if (UserReputationAtTime < 10)
            score -= 15;

        return Math.Max(0, Math.Min(100, score));
    }

    /// <summary>
    /// Spam/bot olma ihtimalini kontrol et
    /// </summary>
    public bool IsSuspiciousVote()
    {
        // Çok hızlı oy (3 saniyeden az)
        if ((VotedAt - CreatedAt).TotalSeconds < 3)
            return true;

        // Güvenilirlik skoru çok düşük
        if (CalculateTrustScore() < 30)
            return true;

        // Aynı IP'den çok fazla oy (başka kontrol gerekir)
        // Bu kontrol repository veya service seviyesinde yapılmalı

        return false;
    }

    /// <summary>
    /// Oy kaynağını güncelle
    /// </summary>
    public void UpdateSource(VoteSource newSource)
    {
        if (Source == newSource)
            return;

        Source = newSource;
        SetModifiedDate();
    }

    /// <summary>
    /// IP adresini maskele (GDPR uyumluluğu)
    /// </summary>
    public void MaskIpAddress()
    {
        if (string.IsNullOrWhiteSpace(IpAddress))
            return;

        var parts = IpAddress.Split('.');
        if (parts.Length == 4) // IPv4
        {
            IpAddress = $"{parts[0]}.{parts[1]}.xxx.xxx";
        }
        else if (IpAddress.Contains(':')) // IPv6
        {
            var v6Parts = IpAddress.Split(':');
            if (v6Parts.Length >= 4)
            {
                IpAddress = $"{v6Parts[0]}:{v6Parts[1]}:xxxx:xxxx:...";
            }
        }

        SetModifiedDate();
    }

    /// <summary>
    /// Oy bilgilerini özetle
    /// </summary>
    public string GetVoteSummary()
    {
        var voteType = IsUpvote ? "👍 Faydalı" : "👎 Faydalı Değil";
        var trustLevel = CalculateTrustScore() switch
        {
            >= 80 => "Yüksek Güven",
            >= 50 => "Orta Güven",
            >= 30 => "Düşük Güven",
            _ => "Şüpheli"
        };

        var summary = $"{voteType} [{trustLevel}]";
        
        if (WasChanged)
            summary += $" (Değiştirildi: {ChangeCount} kez)";
            
        if (IsVerifiedUser)
            summary += " ✓";
            
        return summary;
    }

    /// <summary>
    /// Oy istatistiklerini döndür
    /// </summary>
    public VoteStatistics GetStatistics()
    {
        return new VoteStatistics
        {
            VoteType = IsUpvote ? VoteType.Upvote : VoteType.Downvote,
            VotedAt = VotedAt,
            Source = Source,
            Weight = CalculateVoteWeight(),
            TrustScore = CalculateTrustScore(),
            IsVerifiedUser = IsVerifiedUser,
            ChangeCount = ChangeCount,
            TimeSinceVote = DateTime.UtcNow - VotedAt
        };
    }

    /// <summary>
    /// Oyun yaşını hesapla
    /// </summary>
    public string GetAge()
    {
        var age = DateTime.UtcNow - VotedAt;

        if (age.TotalMinutes < 1)
            return "Az önce";
        if (age.TotalHours < 1)
            return $"{(int)age.TotalMinutes} dakika önce";
        if (age.TotalDays < 1)
            return $"{(int)age.TotalHours} saat önce";
        if (age.TotalDays < 7)
            return $"{(int)age.TotalDays} gün önce";
        if (age.TotalDays < 30)
            return $"{(int)(age.TotalDays / 7)} hafta önce";
        if (age.TotalDays < 365)
            return $"{(int)(age.TotalDays / 30)} ay önce";

        return $"{(int)(age.TotalDays / 365)} yıl önce";
    }

    /// <summary>
    /// Oy kaynak açıklamasını döndür
    /// </summary>
    public string GetSourceDescription()
    {
        return Source switch
        {
            VoteSource.Direct => "Yorum Sayfası",
            VoteSource.ReviewList => "Yorum Listesi",
            VoteSource.CompanyProfile => "Şirket Profili",
            VoteSource.UserProfile => "Kullanıcı Profili",
            VoteSource.SearchResults => "Arama Sonuçları",
            _ => "Bilinmeyen"
        };
    }

    /// <summary>
    /// Oy istatistikleri için yardımcı sınıf
    /// </summary>
    public class VoteStatistics
    {
        public VoteType VoteType { get; set; }
        public DateTime VotedAt { get; set; }
        public VoteSource Source { get; set; }
        public decimal Weight { get; set; }
        public int TrustScore { get; set; }
        public bool IsVerifiedUser { get; set; }
        public int ChangeCount { get; set; }
        public TimeSpan TimeSinceVote { get; set; }
    }
}