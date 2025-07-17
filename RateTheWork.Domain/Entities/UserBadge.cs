using System.Text.Json;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events.UserBadge;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Kullanıcı rozeti entity'si - Hangi kullanıcının hangi rozeti kazandığını eşleştirmek için.
/// </summary>
public class UserBadge : BaseEntity
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private UserBadge() : base()
    {
    }

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
    /// Kullanıcıya rozet atar (Factory method)
    /// </summary>
    public static UserBadge Award
    (
        string userId
        , string badgeId
        , string? awardReason = null
        , string? specialNote = null
    )
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (string.IsNullOrWhiteSpace(badgeId))
            throw new ArgumentNullException(nameof(badgeId));

        var userBadge = new UserBadge
        {
            UserId = userId, BadgeId = badgeId, AwardedAt = DateTime.UtcNow, AwardReason = awardReason
            , IsDisplayed = true, DisplayOrder = 999, IsNew = true, SpecialNote = specialNote
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
    /// Otomatik sistem rozeti atar
    /// </summary>
    public static UserBadge AwardAutomatic
    (
        string userId
        , string badgeId
        , string triggerCondition
        , Dictionary<string, object>? metadata = null
    )
    {
        var reason = $"Otomatik kazanım: {triggerCondition}";
        var specialNote = metadata != null
            ? JsonSerializer.Serialize(metadata)
            : null;

        var userBadge = Award(userId, badgeId, reason, specialNote);

        // Ek domain event ekleyebiliriz
        userBadge.AddDomainEvent(new AutomaticBadgeAwardedEvent(
            userBadge.Id,
            userId,
            badgeId,
            triggerCondition,
            metadata,
            DateTime.UtcNow
        ));

        return userBadge;
    }

    /// <summary>
    /// Milestone rozeti atar (belirli bir başarı için)
    /// </summary>
    public static UserBadge AwardMilestone
    (
        string userId
        , string badgeId
        , string milestoneName
        , int achievedValue
        , int requiredValue
    )
    {
        var reason = $"{milestoneName}: {achievedValue}/{requiredValue} tamamlandı";
        var specialNote = $"Başarı değeri: {achievedValue}";

        return Award(userId, badgeId, reason, specialNote);
    }

    /// <summary>
    /// Sezonluk/dönemsel rozet atar
    /// </summary>
    public static UserBadge AwardSeasonal
    (
        string userId
        , string badgeId
        , string seasonName
        , DateTime seasonStart
        , DateTime seasonEnd
    )
    {
        var reason = $"{seasonName} sezonu rozeti";
        var specialNote = $"Sezon: {seasonStart:yyyy-MM-dd} - {seasonEnd:yyyy-MM-dd}";

        var userBadge = Award(userId, badgeId, reason, specialNote);

        // Sezonluk rozetler için özel event
        userBadge.AddDomainEvent(new SeasonalBadgeAwardedEvent(
            userBadge.Id,
            userId,
            badgeId,
            seasonName,
            seasonStart,
            seasonEnd,
            DateTime.UtcNow
        ));

        return userBadge;
    }

    /// <summary>
    /// Admin tarafından manuel rozet atar
    /// </summary>
    public static UserBadge AwardManual
    (
        string userId
        , string badgeId
        , string adminId
        , string reason
        , string? specialNote = null
    )
    {
        var fullReason = $"Admin tarafından verildi: {reason}";
        var note = $"Veren Admin: {adminId}. {specialNote ?? ""}";

        var userBadge = Award(userId, badgeId, fullReason, note);

        // Manuel rozet için özel event
        userBadge.AddDomainEvent(new ManualBadgeAwardedEvent(
            userBadge.Id,
            userId,
            badgeId,
            adminId,
            reason,
            DateTime.UtcNow
        ));

        return userBadge;
    }

    /// <summary>
    /// Başarı rozeti atar (örn: ilk yorum, 100. yorum vb.)
    /// </summary>
    public static UserBadge AwardAchievement
    (
        string userId
        , string badgeId
        , string achievementType
        , string achievementDetails
    )
    {
        var reason = $"Başarı kilidi açıldı: {achievementType}";
        var specialNote = achievementDetails;

        return Award(userId, badgeId, reason, specialNote);
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
