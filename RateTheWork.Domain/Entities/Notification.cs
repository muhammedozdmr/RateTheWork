using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.Notification;
using RateTheWork.Domain.Events.Notification;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Bildirim entity'si - Kullanıcılara gönderilen bildirimleri temsil eder.
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Notification() : base()
    {
    }


    // Properties
    public string UserId { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public string TypeString => Type.ToString();
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
    /// Yeni bildirim oluşturur (Factory method)
    /// </summary>
    public static Notification Create
    (
        string userId
        , NotificationType type
        , string? title = null
        , string? message = null
        , NotificationPriority? priority = null
        , NotificationChannel? channels = null
        , string? relatedEntityType = null
        , string? relatedEntityId = null
        , string? actionUrl = null
        , string? imageUrl = null
        , Dictionary<string, object>? data = null
        , int? expirationDays = null
    )
    {
        // Varsayılan değerleri al
        var actualTitle = title ?? type.GetDefaultTitle();
        var actualMessage = message ?? $"{type.GetDefaultTitle()} bildirimi";
        var actualPriority = priority ?? type.GetDefaultPriority();
        var actualChannels = channels ?? type.GetDefaultChannels();

        ValidateTitle(actualTitle);
        ValidateMessage(actualMessage);

        var notification = new Notification
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId)), Type = type, Title = actualTitle
            , Message = actualMessage, Priority = actualPriority, Channels = actualChannels
            , RelatedEntityType = relatedEntityType, RelatedEntityId = relatedEntityId, ActionUrl = actionUrl
            , ImageUrl = imageUrl, Data = data
            , ExpiresAt = expirationDays.HasValue ? DateTime.UtcNow.AddDays(expirationDays.Value) : null
        };

        // Domain Event
        notification.AddDomainEvent(new NotificationCreatedEvent(
            notification.Id,
            userId,
            type.ToString(),
            actualTitle,
            actualPriority.ToString(),
            DateTime.UtcNow
        ));

        return notification;
    }


    /// <summary>
    /// Toplu bildirim oluşturur (Factory method)
    /// </summary>
    public static List<Notification> CreateBulk
    (
        string[] userIds
        , NotificationType type
        , string? title = null
        , string? message = null
        , NotificationPriority? priority = null
        , NotificationChannel? channels = null
    )
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
            var firstNotification = notifications.First();
            notifications.First().AddDomainEvent(new BulkNotificationSentEvent(
                userIds,
                type.ToString(),
                firstNotification.Title,
                userIds.Length,
                DateTime.UtcNow
            ));
        }

        return notifications;
    }

    //TODO: Burayı yapmayı unutma factory metodları genişlet

    // /// <summary>
    // /// Sistem duyurusu oluşturur (tüm kullanıcılara)
    // /// </summary>
    // public static List<Notification> CreateSystemAnnouncement(
    //     string[] allUserIds,
    //     string title,
    //     string message,
    //     DateTime? expiresAt = null)
    // {
    //     return allUserIds.Select(userId => Create(
    //         userId,
    //         NotificationTypes.SystemAnnouncement,
    //         title,
    //         message,
    //         NotificationPriority.High,
    //         NotificationChannel.All, // Tüm kanallardan gönder
    //         expirationDays: expiresAt.HasValue ? 
    //             (int)(expiresAt.Value - DateTime.UtcNow).TotalDays : 
    //             DomainConstants.Notification.DefaultExpirationDays
    //     )).ToList();
    // }
    //
    // /// <summary>
    // /// Template'den bildirim oluşturur
    // /// </summary>
    // public static Notification CreateFromTemplate(
    //     string userId,
    //     NotificationTemplate template,
    //     Dictionary<string, string> parameters)
    // {
    //     // Template'deki placeholder'ları doldur
    //     var title = ReplacePlaceholders(template.Title, parameters);
    //     var message = ReplacePlaceholders(template.Message, parameters);
    //
    //     return Create(
    //         userId,
    //         template.Type,
    //         title,
    //         message,
    //         template.Priority,
    //         template.Channels,
    //         data: parameters.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value)
    //     );
    // }
    //
    // /// <summary>
    // /// Zamanlı bildirim oluşturur (ileride gönderilecek)
    // /// </summary>
    // public static ScheduledNotification CreateScheduled(
    //     string userId,
    //     string type,
    //     string title,
    //     string message,
    //     DateTime scheduledFor,
    //     NotificationChannel channels = NotificationChannel.InApp)
    // {
    //     var notification = Create(userId, type, title, message, channels: channels);
    //     
    //     return new ScheduledNotification
    //     {
    //         Notification = notification,
    //         ScheduledFor = scheduledFor,
    //         Status = "Pending"
    //     };
    // }

    private static string ReplacePlaceholders(string template, Dictionary<string, string> parameters)
    {
        foreach (var param in parameters)
        {
            template = template.Replace($"{{{param.Key}}}", param.Value);
        }

        return template;
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
        if (!Channels.HasFlag(NotificationChannel.Sms))
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
}
