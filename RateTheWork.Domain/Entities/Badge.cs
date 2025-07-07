using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Rozet entity'si - Kazanılabilir rozetlerin tanımlarını tutar.
/// </summary>
public class Badge : BaseEntity
{
    // Badge Types
    public enum BadgeType
    {
        FirstReview,        // İlk yorum
        ActiveReviewer,     // Aktif yorumcu (10+ yorum)
        TrustedReviewer,    // Güvenilir yorumcu (5+ doğrulanmış yorum)
        TopContributor,    // En çok katkıda bulunan (50+ yorum)
        CompanyExplorer,   // Farklı şirketlerde yorum yapan (10+ şirket)
        DetailedReviewer,  // Detaylı yorumlar yazan (ortalama 500+ karakter)
        HelpfulReviewer,   // Faydalı yorumlar (upvote oranı %80+)
        Anniversary,       // Platform yıldönümü
        SpecialEvent      // Özel etkinlik rozeti
    }

    // Badge Rarity Levels
    public enum BadgeRarity
    {
        Common,      // Herkes kazanabilir
        Uncommon,    // Biraz çaba gerektirir
        Rare,        // Zor kazanılır
        Epic,        // Çok zor kazanılır
        Legendary    // Efsanevi seviye
    }

    // Properties
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string IconUrl { get; private set; }
    public string Criteria { get; private set; }
    public BadgeType Type { get; private set; }
    public BadgeRarity Rarity { get; private set; }
    public int RequiredCount { get; private set; } // Kaç adet gerekli (yorum sayısı vb.)
    public bool IsActive { get; private set; }
    public DateTime? AvailableFrom { get; private set; }
    public DateTime? AvailableUntil { get; private set; }
    public int Points { get; private set; } // Rozet puanı (gamification için)

    /// <summary>
    /// EF Core için private constructor
    /// </summary>
    private Badge(string name, string description, string ıconUrl, string criteria) : base()
    {
        Name = name;
        Description = description;
        IconUrl = ıconUrl;
        Criteria = criteria;
    }

    /// <summary>
    /// Yeni rozet oluşturur
    /// </summary>
    public static Badge Create(
        string name,
        string description,
        string iconUrl,
        string criteria,
        BadgeType type,
        BadgeRarity rarity,
        int requiredCount = 0,
        int points = 10)
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidateIconUrl(iconUrl);

        var badge = new Badge
        {
            Name = name,
            Description = description,
            IconUrl = iconUrl,
            Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria)),
            Type = type,
            Rarity = rarity,
            RequiredCount = requiredCount,
            IsActive = true,
            Points = points
        };

        // Rozet tipine göre varsayılan değerler
        badge.SetDefaultValuesByType();

        return badge;
    }

    /// <summary>
    /// Özel etkinlik rozeti oluşturur
    /// </summary>
    public static Badge CreateSpecialEventBadge(
        string name,
        string description,
        string iconUrl,
        string criteria,
        DateTime availableFrom,
        DateTime availableUntil,
        int points = 50)
    {
        var badge = Create(name, description, iconUrl, criteria, BadgeType.SpecialEvent, BadgeRarity.Epic, 0, points);
        badge.SetAvailabilityPeriod(availableFrom, availableUntil);
        return badge;
    }

    /// <summary>
    /// Kullanıcının rozeti kazanıp kazanamayacağını kontrol eder
    /// </summary>
    public bool CheckEligibility(
        int userReviewCount,
        int userVerifiedReviewCount,
        int userCompanyCount,
        double userAverageCommentLength,
        double userUpvoteRatio,
        DateTime userRegistrationDate,
        int userWarningCount)
    {
        // Aktif değilse kazanılamaz
        if (!IsActive)
            return false;

        // Zaman kontrolü
        if (!IsCurrentlyAvailable())
            return false;

        // Rozet tipine göre kontrol
        return Type switch
        {
            BadgeType.FirstReview => userReviewCount >= 1,
            
            BadgeType.ActiveReviewer => userReviewCount >= RequiredCount,
            
            BadgeType.TrustedReviewer => userVerifiedReviewCount >= RequiredCount,
            
            BadgeType.TopContributor => userReviewCount >= RequiredCount && userWarningCount == 0,
            
            BadgeType.CompanyExplorer => userCompanyCount >= RequiredCount,
            
            BadgeType.DetailedReviewer => userAverageCommentLength >= 500 && userReviewCount >= 5,
            
            BadgeType.HelpfulReviewer => userUpvoteRatio >= 0.8 && userReviewCount >= 10,
            
            BadgeType.Anniversary => (DateTime.UtcNow - userRegistrationDate).TotalDays >= 365,
            
            BadgeType.SpecialEvent => true, // Özel mantık gerekebilir
            
            _ => false
        };
    }

    /// <summary>
    /// Rozet şu anda kazanılabilir mi?
    /// </summary>
    public bool IsCurrentlyAvailable()
    {
        var now = DateTime.UtcNow;

        if (AvailableFrom.HasValue && now < AvailableFrom.Value)
            return false;

        if (AvailableUntil.HasValue && now > AvailableUntil.Value)
            return false;

        return IsActive;
    }

    /// <summary>
    /// Rozet süresini ayarlar
    /// </summary>
    public void SetAvailabilityPeriod(DateTime from, DateTime until)
    {
        if (from >= until)
            throw new BusinessRuleException("Başlangıç tarihi bitiş tarihinden önce olmalıdır.");

        AvailableFrom = from;
        AvailableUntil = until;
        SetModifiedDate();
    }

    /// <summary>
    /// Rozeti deaktive eder
    /// </summary>
    public void Deactivate(string reason)
    {
        if (!IsActive)
            return;

        IsActive = false;
        SetModifiedDate();

        AddDomainEvent(new BadgeDeactivatedEvent(Id, Name, reason));
    }

    /// <summary>
    /// Rozeti aktive eder
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        SetModifiedDate();

        AddDomainEvent(new BadgeActivatedEvent(Id, Name));
    }

    /// <summary>
    /// Rozet bilgilerini günceller
    /// </summary>
    public void UpdateInfo(string name, string description, string iconUrl)
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidateIconUrl(iconUrl);

        Name = name;
        Description = description;
        IconUrl = iconUrl;
        SetModifiedDate();
    }

    /// <summary>
    /// Rozet kriterini günceller
    /// </summary>
    public void UpdateCriteria(string criteria, int requiredCount)
    {
        if (string.IsNullOrWhiteSpace(criteria))
            throw new ArgumentNullException(nameof(criteria));

        if (requiredCount < 0)
            throw new BusinessRuleException("Gerekli sayı negatif olamaz.");

        Criteria = criteria;
        RequiredCount = requiredCount;
        SetModifiedDate();
    }

    /// <summary>
    /// Rozet nadir seviyesini döndürür
    /// </summary>
    public string GetRarityColor()
    {
        return Rarity switch
        {
            BadgeRarity.Common => "#808080",      // Gri
            BadgeRarity.Uncommon => "#1EFF00",    // Yeşil
            BadgeRarity.Rare => "#0080FF",        // Mavi
            BadgeRarity.Epic => "#B335F7",        // Mor
            BadgeRarity.Legendary => "#FF8000",   // Turuncu
            _ => "#808080"
        };
    }

    /// <summary>
    /// Rozet açıklamasını zenginleştirir
    /// </summary>
    public string GetDetailedDescription()
    {
        var details = $"{Description}\n\n";
        details += $"Nadir Seviyesi: {GetRarityDisplayName()}\n";
        details += $"Puan Değeri: {Points}\n";
        details += $"Kriterler: {Criteria}";

        if (RequiredCount > 0)
            details += $"\nGerekli Sayı: {RequiredCount}";

        if (AvailableFrom.HasValue || AvailableUntil.HasValue)
        {
            details += "\n\nKazanım Süresi: ";
            if (AvailableFrom.HasValue)
                details += $"{AvailableFrom.Value:dd.MM.yyyy}";
            if (AvailableUntil.HasValue)
                details += $" - {AvailableUntil.Value:dd.MM.yyyy}";
        }

        return details;
    }

    // Private methods
    private void SetDefaultValuesByType()
    {
        switch (Type)
        {
            case BadgeType.FirstReview:
                if (RequiredCount == 0) RequiredCount = 1;
                if (Points == 10) Points = 10;
                break;
            case BadgeType.ActiveReviewer:
                if (RequiredCount == 0) RequiredCount = 10;
                if (Points == 10) Points = 25;
                break;
            case BadgeType.TrustedReviewer:
                if (RequiredCount == 0) RequiredCount = 5;
                if (Points == 10) Points = 50;
                break;
            case BadgeType.TopContributor:
                if (RequiredCount == 0) RequiredCount = 50;
                if (Points == 10) Points = 100;
                break;
            case BadgeType.CompanyExplorer:
                if (RequiredCount == 0) RequiredCount = 10;
                if (Points == 10) Points = 30;
                break;
            case BadgeType.Anniversary:
                if (Points == 10) Points = 75;
                break;
        }
    }

    private string GetRarityDisplayName()
    {
        return Rarity switch
        {
            BadgeRarity.Common => "Sıradan",
            BadgeRarity.Uncommon => "Yaygın Olmayan",
            BadgeRarity.Rare => "Nadir",
            BadgeRarity.Epic => "Epik",
            BadgeRarity.Legendary => "Efsanevi",
            _ => "Bilinmeyen"
        };
    }

    // Validation methods
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (name.Length > 100)
            throw new BusinessRuleException("Rozet adı 100 karakterden uzun olamaz.");
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentNullException(nameof(description));

        if (description.Length > 500)
            throw new BusinessRuleException("Rozet açıklaması 500 karakterden uzun olamaz.");
    }

    private static void ValidateIconUrl(string iconUrl)
    {
        if (string.IsNullOrWhiteSpace(iconUrl))
            throw new ArgumentNullException(nameof(iconUrl));

        if (!Uri.IsWellFormedUriString(iconUrl, UriKind.Absolute))
            throw new BusinessRuleException("Geçersiz ikon URL'i.");
    }
}