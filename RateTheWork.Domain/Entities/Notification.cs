using System.Text.Json;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Bildirim entity'si - Kullanıcılara gönderilen bildirimleri temsil eder.
/// </summary>
public class Notification : BaseEntity
{
    // Notification Types
    public static class NotificationTypes
    {
        // Account
        public const string Welcome = "Account.Welcome";
        public const string EmailVerified = "Account.EmailVerified";
        public const string PhoneVerified = "Account.PhoneVerified";
        public const string PasswordChanged = "Account.PasswordChanged";
        public const string ProfileUpdated = "Account.ProfileUpdated";
        
        // Review
        public const string ReviewApproved = "Review.Approved";
        public const string ReviewRejected = "Review.Rejected";
        public const string ReviewReceivesVote = "Review.ReceivedVote";
        public const string ReviewReceivesComment = "Review.ReceivedComment";
        public const string ReviewReported = "Review.Reported";
        public const string ReviewHidden = "Review.Hidden";
        
        // Document
        public const string DocumentVerified = "Document.Verified";
        public const string DocumentRejected = "Document.Rejected";
        
        // Moderation
        public const string WarningIssued = "Moderation.Warning";
        public const string BanIssued = "Moderation.Ban";
        public const string BanLifted = "Moderation.BanLifted";
        
        // Badge
        public const string BadgeEarned = "Badge.Earned";
        public const string BadgeRemoved = "Badge.Removed";
        
        // Company
        public const string CompanyResponded = "Company.Responded";
        public const string CompanyVerified = "Company.Verified";
        
        // System
        public const string SystemAnnouncement = "System.Announcement";
        public const string SystemMaintenance = "System.Maintenance";
        public const string TermsUpdated = "System.TermsUpdated";
    }

    // Notification Priority
    public enum NotificationPriority
    {
        Low,      // Düşük öncelik - rozet kazanma vb.
        Normal,   // Normal - çoğu bildirim
        High,     // Yüksek - uyarılar vb.
        Critical  // Kritik - ban, güvenlik vb.
    }

    // Properties
    public string UserId { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; } = false;
    public DateTime? ReadAt { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public string? RelatedEntityId { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public string? ActionUrl { get; private set; }
    public string? ImageUrl { get; private set; }
    public Dictionary<string, object>? Data { get; private set; }
    public NotificationChannel Channels { get; private set; }
    public bool IsEmailSent { get; private set; } = false;
    public bool IsSmsSent { get; private set; } = false;
    public bool IsPushSent { get; private set; } = false;
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Notification() : base()
    {
    }

    /// <summary>
    /// Yeni bildirim oluşturur (Factory method)
    /// </summary>
    public static Notification Create(
        string userId,
        string type,
        string title,
        string message,
        NotificationPriority priority = NotificationPriority.Normal,
        NotificationChannel channels = NotificationChannel.InApp,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        string? actionUrl = null,
        string? imageUrl = null,
        Dictionary<string, object>? data = null,
        int? expirationDays = null)
    {
        ValidateType(type);
        ValidateTitle(title);
        ValidateMessage(message);

        var notification = new Notification
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId)),
            Type = type,
            Title = title,
            Message = message,
            Priority = priority,
            Channels = channels,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            ActionUrl = actionUrl,
            ImageUrl = imageUrl,
            Data = data,
            ExpiresAt = expirationDays.HasValue ? DateTime.UtcNow.AddDays(expirationDays.Value) : null
        };

        // Domain Event
        notification.AddDomainEvent(new NotificationCreatedEvent(
            notification.Id,
            userId,
            type,
            title,
            priority.ToString(),
            DateTime.UtcNow,
            DateTime.UtcNow
        ));

        return notification;
    }

    /// <summary>
    /// Toplu bildirim oluşturur (Factory method)
    /// </summary>
    public static List<Notification> CreateBulk(
        string[] userIds,
        string type,
        string title,
        string message,
        NotificationPriority priority = NotificationPriority.Normal,
        NotificationChannel channels = NotificationChannel.InApp)
    {
        if (userIds == null || userIds.Length == 0)
            throw new ArgumentException("En az bir kullanıcı ID'si gerekli.", nameof(userIds));

        var notifications = new List<Notification>();

        foreach (var userId in userIds.Distinct())
        {
            notifications.Add(Create(userId, type, title, message, priority, channels));
        }

        // Bulk notification event - ilk notification'a ekle
        if (notifications.Any())
        {
            notifications.First().AddDomainEvent(new BulkNotificationSentEvent(
                userIds,
                type,
                title,
                userIds.Length,
                DateTime.UtcNow,
                DateTime.UtcNow
            ));
        }

        return notifications;
    }

    /// <summary>
    /// Bildirimi okundu olarak işaretle
    /// </summary>
    public void MarkAsRead()
    {
        if (IsRead)
            throw new BusinessRuleException("Bildirim zaten okunmuş.");

        IsRead = true;
        ReadAt = DateTime.UtcNow;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new NotificationReadEvent(
            Id,
            UserId,
            DateTime.UtcNow,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Email gönderim durumunu güncelle
    /// </summary>
    public void MarkEmailSent()
    {
        if (!Channels.HasFlag(NotificationChannel.Email))
            throw new BusinessRuleException("Bu bildirim email kanalını içermiyor.");

        IsEmailSent = true;
        SetModifiedDate();
    }

    /// <summary>
    /// SMS gönderim durumunu güncelle
    /// </summary>
    public void MarkSmsSent()
    {
        if (!Channels.HasFlag(NotificationChannel.SMS))
            throw new BusinessRuleException("Bu bildirim SMS kanalını içermiyor.");

        IsSmsSent = true;
        SetModifiedDate();
    }

    /// <summary>
    /// Push notification gönderim durumunu güncelle
    /// </summary>
    public void MarkPushSent()
    {
        if (!Channels.HasFlag(NotificationChannel.Push))
            throw new BusinessRuleException("Bu bildirim push kanalını içermiyor.");

        IsPushSent = true;
        SetModifiedDate();
    }

    /// <summary>
    /// Bildirim süresi dolmuş mu kontrol et
    /// </summary>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
    }

    /// <summary>
    /// Data dictionary'sine veri ekle
    /// </summary>
    public void AddData(string key, object value)
    {
        Data ??= new Dictionary<string, object>();
        Data[key] = value;
        SetModifiedDate();
    }

    // Private validation methods
    private static void ValidateType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentNullException(nameof(type));

        if (type.Length > 50)
            throw new BusinessRuleException("Bildirim tipi 50 karakterden uzun olamaz.");
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentNullException(nameof(title));

        if (title.Length > 100)
            throw new BusinessRuleException("Bildirim başlığı 100 karakterden uzun olamaz.");
    }

    private static void ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));

        if (message.Length > 500)
            throw new BusinessRuleException("Bildirim mesajı 500 karakterden uzun olamaz.");
    }
}
