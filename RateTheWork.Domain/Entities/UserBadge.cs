using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Events.UserBadge;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Kullanıcı rozeti entity'si - Hangi kullanıcının hangi rozeti kazandığını eşleştirmek için.
/// </summary>
public class UserBadge : BaseEntity
{
    // Properties
    public string? UserId { get; private set; } = string.Empty;
    public string? BadgeId { get; private set; } = string.Empty;
    public DateTime AwardedAt { get; private set; }
    public string? AwardReason { get; private set; } = string.Empty;
    public bool IsDisplayed { get; private set; } = true;
    public int DisplayOrder { get; private set; } = 999;
    public bool IsNew { get; private set; } = true;
    public DateTime? ViewedAt { get; private set; }
    public string? SpecialNote { get; private set; } = string.Empty;

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private UserBadge() : base()
    {
    }

    /// <summary>
    /// Kullanıcıya rozet atar (Factory method)
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
            IsDisplayed = true,
            DisplayOrder = 999,
            IsNew = true,
            SpecialNote = specialNote
        };

        // Domain Event
        userBadge.AddDomainEvent(new BadgeAwardedEvent(
            userBadge.Id,
            userId,
            badgeId,
            userBadge.AwardedAt,
            awardReason,
            specialNote,
            DateTime.UtcNow
        ));

        return userBadge;
    }

    /// <summary>
    /// Rozeti görüntülendi olarak işaretle
    /// </summary>
    public void MarkAsViewed()
    {
        if (!IsNew)
            throw new BusinessRuleException("Rozet zaten görüntülenmiş.");

        IsNew = false;
        ViewedAt = DateTime.UtcNow;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new BadgeViewedEvent(
            Id,
            UserId,
            BadgeId,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Rozeti göster
    /// </summary>
    public void Display()
    {
        if (IsDisplayed)
            throw new BusinessRuleException("Rozet zaten görünür durumda.");

        IsDisplayed = true;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new BadgeDisplayedEvent(
            Id,
            UserId,
            BadgeId,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Rozeti gizle
    /// </summary>
    public void Hide()
    {
        if (!IsDisplayed)
            throw new BusinessRuleException("Rozet zaten gizli.");

        IsDisplayed = false;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new BadgeHiddenEvent(
            Id,
            UserId,
            BadgeId,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Görüntüleme sırasını güncelle
    /// </summary>
    public void UpdateDisplayOrder(int newOrder)
    {
        if (newOrder < 0)
            throw new BusinessRuleException("Görüntüleme sırası negatif olamaz.");

        DisplayOrder = newOrder;
        SetModifiedDate();
    }

    /// <summary>
    /// Özel not güncelle
    /// </summary>
    public void UpdateSpecialNote(string? specialNote)
    {
        SpecialNote = specialNote;
        SetModifiedDate();
    }
}