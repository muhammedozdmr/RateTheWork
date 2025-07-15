using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.Badge;
using RateTheWork.Domain.Events.Badge;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Rozet entity'si - Kazanılabilir rozetlerin tanımlarını tutar.
/// </summary>
public class Badge : BaseEntity
{
    // Properties
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string IconUrl { get; private set; } = string.Empty;
    public string? Criteria { get; private set; } = string.Empty;
    public BadgeType Type { get; private set; }
    public BadgeRarity Rarity { get; private set; }
    public int RequiredCount { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? AvailableFrom { get; private set; }
    public DateTime? AvailableUntil { get; private set; }
    public int Points { get; private set; }

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Badge() : base()
    {
    }

    /// <summary>
    /// Yeni rozet oluşturur (Factory method)
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
            Points = points > 0 ? points : 10,
            IsActive = true
        };

        // Domain Event
        badge.AddDomainEvent(new BadgeCreatedEvent(
            badge.Id,
            badge.Name,
            badge.Type.ToString(),
            badge.Rarity.ToString(),
            DateTime.UtcNow
        ));

        return badge;
    }

    /// <summary>
    /// Rozeti aktifleştir
    /// </summary>
    public void Activate(string activatedBy)
    {
        if (IsActive)
            throw new BusinessRuleException("Rozet zaten aktif.");

        IsActive = true;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new BadgeActivatedEvent(
            Id,
            Name,
            activatedBy,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Rozeti deaktive et
    /// </summary>
    public void Deactivate(string deactivatedBy, string reason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Rozet zaten deaktif.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        IsActive = false;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new BadgeDeactivatedEvent(
            Id,
            Name,
            deactivatedBy,
            reason,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Rozet gereksinimlerini güncelle
    /// </summary>
    public void UpdateRequirements(int requiredCount, string criteria)
    {
        if (requiredCount < 0)
            throw new BusinessRuleException("Gereksinim sayısı negatif olamaz.");

        if (string.IsNullOrWhiteSpace(criteria))
            throw new ArgumentNullException(nameof(criteria));

        RequiredCount = requiredCount;
        Criteria = criteria;
        SetModifiedDate();
    }

    /// <summary>
    /// Rozet süresini ayarla
    /// </summary>
    public void SetAvailabilityPeriod(DateTime? from, DateTime? until)
    {
        if (from.HasValue && until.HasValue && from.Value >= until.Value)
            throw new BusinessRuleException("Başlangıç tarihi bitiş tarihinden önce olmalıdır.");

        AvailableFrom = from;
        AvailableUntil = until;
        SetModifiedDate();
    }

    /// <summary>
    /// Rozet şu anda kazanılabilir mi?
    /// </summary>
    public bool IsAvailable()
    {
        if (!IsActive)
            return false;

        var now = DateTime.UtcNow;

        if (AvailableFrom.HasValue && now < AvailableFrom.Value)
            return false;

        if (AvailableUntil.HasValue && now > AvailableUntil.Value)
            return false;

        return true;
    }

    // Private validation methods
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

        if (!Uri.TryCreate(iconUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new BusinessRuleException("Geçersiz ikon URL'i.");
        }
    }
}