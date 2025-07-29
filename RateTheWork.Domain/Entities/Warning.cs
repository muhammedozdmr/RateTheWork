using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.User;
using RateTheWork.Domain.Events.Warning;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Kullanıcı uyarı entity'si
/// </summary>
public class Warning : BaseEntity
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Warning() : base()
    {
    }

    // Properties
    public string UserId { get; private set; } = string.Empty;
    public string AdminUserId { get; private set; } = string.Empty;
    public WarningType Type { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? RelatedEntityType { get; private set; }
    public string? RelatedEntityId { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsAcknowledged { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }

    /// <summary>
    /// Yeni uyarı oluşturur (Factory method)
    /// </summary>
    public static Warning Create(
        string userId,
        string adminUserId,
        WarningType type,
        string reason,
        int validityDays = 90,
        string? relatedEntityType = null,
        string? relatedEntityId = null)
    {
        // Validasyonlar
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));
            
        if (string.IsNullOrWhiteSpace(adminUserId))
            throw new ArgumentNullException(nameof(adminUserId));
            
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));
            
        if (reason.Length < 10)
            throw new BusinessRuleException("Uyarı açıklaması en az 10 karakter olmalıdır.");
            
        if (reason.Length > 500)
            throw new BusinessRuleException("Uyarı açıklaması 500 karakterden uzun olamaz.");

        if (validityDays <= 0)
            throw new BusinessRuleException("Geçerlilik süresi 0'dan büyük olmalıdır.");

        var warning = new Warning
        {
            UserId = userId,
            AdminUserId = adminUserId,
            Type = type,
            Reason = reason,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(validityDays)
        };

        // Domain event
        warning.AddDomainEvent(new WarningIssuedEvent(
            warning.Id,
            userId,
            adminUserId,
            type.ToString(),
            reason,
            warning.IssuedAt
        ));

        return warning;
    }

    /// <summary>
    /// Uyarıyı onayla/kabul et
    /// </summary>
    public void Acknowledge()
    {
        if (IsAcknowledged)
            throw new BusinessRuleException("Bu uyarı zaten onaylanmış.");

        IsAcknowledged = true;
        AcknowledgedAt = DateTime.UtcNow;
        SetModifiedDate();
    }

    /// <summary>
    /// Uyarıyı iptal et
    /// </summary>
    public void Revoke(string adminUserId, string reason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Bu uyarı zaten aktif değil.");

        IsActive = false;
        SetModifiedDate();

        // Domain event
        AddDomainEvent(new WarningRevokedEvent(
            Id,
            UserId,
            adminUserId,
            reason,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Uyarının süresi dolmuş mu?
    /// </summary>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }
}