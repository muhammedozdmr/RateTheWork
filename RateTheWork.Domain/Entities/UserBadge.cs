using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Kullanıcı rozeti entity'si - Hangi kullanıcının hangi rozeti kazandığını eşleştirmek için.
/// </summary>
public class UserBadge : BaseEntity
{
    // Properties
    public string? UserId { get; private set; }
    public string? BadgeId { get; private set; }
    public DateTime AwardedAt { get; private set; }
    public string? AwardReason { get; private set; } // Neden kazandı (özel durumlar için)
    public bool IsDisplayed { get; private set; } // Kullanıcı profilinde görünsün mü?
    public int DisplayOrder { get; private set; } // Görüntüleme sırası
    public bool IsNew { get; private set; } // Kullanıcı henüz görmedi mi?
    public DateTime? ViewedAt { get; private set; } // Kullanıcı ne zaman gördü?
    public string? SpecialNote { get; private set; } // Özel not (örn: "2024 yılının en aktif yorumcusu")
    
    // Navigation için referanslar (lazy loading için kullanılabilir)
    private User? _user;
    private Badge? _badge;
    
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private UserBadge() : base()
    {
    }

    /// <summary>
    /// EF Core için private constructor
    /// </summary>
    private UserBadge(string? userId, string? badgeId) : base()
    {
        UserId = userId;
        BadgeId = badgeId;
    }

    /// <summary>
    /// Kullanıcıya rozet atar
    /// </summary>
    public static UserBadge Award(
        string userId,
        string badgeId,
        string? awardReason = null,
        string? specialNote = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (string.IsNullOrWhiteSpace(badgeId))
            throw new ArgumentNullException(nameof(badgeId));

        var userBadge = new UserBadge
        {
            UserId = userId,
            BadgeId = badgeId,
            AwardedAt = DateTime.UtcNow,
            AwardReason = awardReason,
            IsDisplayed = true, // Varsayılan olarak görünür
            DisplayOrder = 999, // En sona ekle
            IsNew = true,
            SpecialNote = specialNote
        };

        // Domain Event
        userBadge.AddDomainEvent(new BadgeAwardedEvent(
            userBadge.Id,
            userId,
            badgeId,
            userBadge.AwardedAt,
            awardReason
        ));

        return userBadge;
    }

    /// <summary>
    /// Rozeti görüntülendi olarak işaretle
    /// </summary>
    public void MarkAsViewed()
    {
        if (!IsNew)
            return;

        IsNew = false;
        ViewedAt = DateTime.UtcNow;
        SetModifiedDate();

        AddDomainEvent(new BadgeViewedEvent(Id, UserId, BadgeId));
    }

    /// <summary>
    /// Rozet görünürlüğünü değiştir
    /// </summary>
    public void ToggleDisplay()
    {
        IsDisplayed = !IsDisplayed;
        SetModifiedDate();

        if (IsDisplayed)
        {
            AddDomainEvent(new BadgeDisplayedEvent(Id, UserId, BadgeId));
        }
        else
        {
            AddDomainEvent(new BadgeHiddenEvent(Id, UserId, BadgeId));
        }
    }

    /// <summary>
    /// Rozeti göster
    /// </summary>
    public void Display()
    {
        if (IsDisplayed)
            return;

        IsDisplayed = true;
        SetModifiedDate();
        AddDomainEvent(new BadgeDisplayedEvent(Id, UserId, BadgeId));
    }

    /// <summary>
    /// Rozeti gizle
    /// </summary>
    public void Hide()
    {
        if (!IsDisplayed)
            return;

        IsDisplayed = false;
        SetModifiedDate();
        AddDomainEvent(new BadgeHiddenEvent(Id, UserId, BadgeId));
    }

    /// <summary>
    /// Görüntüleme sırasını güncelle
    /// </summary>
    public void UpdateDisplayOrder(int newOrder)
    {
        if (newOrder < 0)
            throw new BusinessRuleException("Görüntüleme sırası negatif olamaz.");

        if (DisplayOrder == newOrder)
            return;

        DisplayOrder = newOrder;
        SetModifiedDate();
    }

    /// <summary>
    /// Özel not ekle/güncelle
    /// </summary>
    public void UpdateSpecialNote(string? note)
    {
        if (note?.Length > 200)
            throw new BusinessRuleException("Özel not 200 karakterden uzun olamaz.");

        SpecialNote = note;
        SetModifiedDate();
    }

    /// <summary>
    /// Rozet bilgilerini yükle (navigation property)
    /// </summary>
    public void LoadBadge(Badge badge)
    {
        _badge = badge ?? throw new ArgumentNullException(nameof(badge));
        
        if (_badge.Id != BadgeId)
            throw new BusinessRuleException("Yanlış rozet yüklendi.");
    }

    /// <summary>
    /// Kullanıcı bilgilerini yükle (navigation property)
    /// </summary>
    public void LoadUser(User user)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        
        if (_user.Id != UserId)
            throw new BusinessRuleException("Yanlış kullanıcı yüklendi.");
    }

    /// <summary>
    /// Rozet detaylarını getir
    /// </summary>
    public string GetDisplayInfo()
    {
        if (_badge == null)
            return $"Rozet #{BadgeId} - {AwardedAt:dd.MM.yyyy}";

        var info = $"{_badge.Name} - {AwardedAt:dd.MM.yyyy}";
        
        if (!string.IsNullOrWhiteSpace(SpecialNote))
            info += $" ({SpecialNote})";
            
        return info;
    }

    /// <summary>
    /// Kazanım süresini hesapla
    /// </summary>
    public TimeSpan GetTimeSinceAwarded()
    {
        return DateTime.UtcNow - AwardedAt;
    }

    /// <summary>
    /// Rozet ne kadar süredir yeni durumda?
    /// </summary>
    public TimeSpan? GetTimeSinceNew()
    {
        if (!IsNew || !ViewedAt.HasValue)
            return null;

        return ViewedAt.Value - AwardedAt;
    }

    /// <summary>
    /// Rozet kazanımının yıldönümü mü?
    /// </summary>
    public bool IsAnniversary()
    {
        var daysSinceAwarded = (DateTime.UtcNow - AwardedAt).TotalDays;
        
        // 365 günün katları (yıllık)
        return daysSinceAwarded > 0 && daysSinceAwarded % 365 < 1;
    }

    /// <summary>
    /// Kazanım yaşını döndür
    /// </summary>
    public string GetAgeDescription()
    {
        var age = DateTime.UtcNow - AwardedAt;

        if (age.TotalDays < 1)
            return "Bugün kazanıldı";
        
        if (age.TotalDays < 7)
            return $"{(int)age.TotalDays} gün önce";
            
        if (age.TotalDays < 30)
            return $"{(int)(age.TotalDays / 7)} hafta önce";
            
        if (age.TotalDays < 365)
            return $"{(int)(age.TotalDays / 30)} ay önce";
            
        var years = (int)(age.TotalDays / 365);
        return years == 1 ? "1 yıl önce" : $"{years} yıl önce";
    }

    /// <summary>
    /// Rozet yeni mi? (7 günden yeni)
    /// </summary>
    public bool IsRecentlyAwarded()
    {
        return (DateTime.UtcNow - AwardedAt).TotalDays <= 7;
    }

    /// <summary>
    /// Özel bir rozet mi? (özel not var mı)
    /// </summary>
    public bool IsSpecial()
    {
        return !string.IsNullOrWhiteSpace(SpecialNote);
    }
}