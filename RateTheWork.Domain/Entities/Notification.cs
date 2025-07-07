using System.Text.Json;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Bildirim entity'si - Kullanıcılara gönderilen her bir bildirimi kaydetmek için.
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
    public string? UserId { get; private set; }
    public string? Type { get; private set; }
    public string? Title { get; private set; }
    public string? Message { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public string? RelatedEntityId { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public string? ActionUrl { get; private set; } // Tıklandığında gidilecek URL
    public string? IconType { get; private set; } // UI için ikon tipi
    public DateTime? ExpiresAt { get; private set; } // Ne zaman geçersiz olacak
    public bool IsDeleted { get; private set; } // Kullanıcı sildi mi?
    public string? Data { get; private set; } // Ek veri (JSON)
    public bool RequiresAction { get; private set; } // Kullanıcı aksiyonu gerekli mi?
    public bool WasSent { get; private set; } // Email/SMS vs. gönderildi mi?
    public DateTime? SentAt { get; private set; }
    
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Notification() : base()
    {
    }

    /// <summary>
    /// EF Core için private constructor
    /// </summary>
    private Notification(string? userId, string? type, string? title, string? message) : base()
    {
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
    }

    /// <summary>
    /// Yeni bildirim oluşturur
    /// </summary>
    public static Notification Create(
        string userId,
        string type,
        string title,
        string message,
        NotificationPriority priority = NotificationPriority.Normal,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        string? actionUrl = null,
        bool requiresAction = false)
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
            IsRead = false,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            Priority = priority,
            ActionUrl = actionUrl,
            RequiresAction = requiresAction,
            IsDeleted = false,
            WasSent = false
        };

        // Type'a göre varsayılan değerler
        notification.SetDefaultsByType();

        // Domain Event
        notification.AddDomainEvent(new NotificationCreatedEvent(
            notification.Id,
            userId,
            type,
            priority
        ));

        return notification;
    }

    /// <summary>
    /// Hoş geldiniz bildirimi oluşturur
    /// </summary>
    public static Notification CreateWelcomeNotification(string userId, string username)
    {
        return Create(
            userId,
            NotificationTypes.Welcome,
            "RateTheWork'e Hoş Geldiniz! 🎉",
            $"Merhaba {username}, aramıza hoş geldin! Şirket değerlendirmelerini inceleyebilir ve kendi deneyimlerini paylaşabilirsin.",
            NotificationPriority.Normal,
            "User",
            userId,
            "/profile"
        );
    }

    /// <summary>
    /// Yorum onaylandı bildirimi
    /// </summary>
    public static Notification CreateReviewApprovedNotification(
        string userId,
        string companyName,
        string reviewId)
    {
        return Create(
            userId,
            NotificationTypes.ReviewApproved,
            "Yorumunuz Onaylandı ✅",
            $"{companyName} hakkındaki yorumunuz onaylandı ve yayında!",
            NotificationPriority.Normal,
            "Review",
            reviewId,
            $"/reviews/{reviewId}"
        );
    }

    /// <summary>
    /// Uyarı bildirimi
    /// </summary>
    public static Notification CreateWarningNotification(
        string userId,
        string warningReason,
        string warningId)
    {
        var notification = Create(
            userId,
            NotificationTypes.WarningIssued,
            "Uyarı Aldınız ⚠️",
            $"Uyarı nedeni: {warningReason}. Lütfen platform kurallarına uygun davranın.",
            NotificationPriority.High,
            "Warning",
            warningId,
            $"/warnings/{warningId}",
            true // Requires action
        );

        notification.ExpiresAt = DateTime.UtcNow.AddDays(30); // 30 gün görünür
        return notification;
    }

    /// <summary>
    /// Rozet kazandı bildirimi
    /// </summary>
    public static Notification CreateBadgeEarnedNotification(
        string userId,
        string badgeName,
        string badgeId)
    {
        return Create(
            userId,
            NotificationTypes.BadgeEarned,
            "Yeni Rozet Kazandınız! 🏆",
            $"Tebrikler! '{badgeName}' rozetini kazandınız.",
            NotificationPriority.Low,
            "Badge",
            badgeId,
            $"/badges/{badgeId}"
        );
    }

    /// <summary>
    /// Sistem duyurusu
    /// </summary>
    public static Notification CreateSystemAnnouncement(
        string userId,
        string title,
        string message,
        DateTime? expiresAt = null)
    {
        var notification = Create(
            userId,
            NotificationTypes.SystemAnnouncement,
            title,
            message,
            NotificationPriority.Normal
        );

        notification.ExpiresAt = expiresAt;
        notification.IconType = "info";
        return notification;
    }

    /// <summary>
    /// Bildirimi okundu olarak işaretle
    /// </summary>
    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
        SetModifiedDate();

        AddDomainEvent(new NotificationReadEvent(Id, UserId));
    }

    /// <summary>
    /// Bildirimi okunmadı olarak işaretle
    /// </summary>
    public void MarkAsUnread()
    {
        if (!IsRead)
            return;

        IsRead = false;
        ReadAt = null;
        SetModifiedDate();
    }

    /// <summary>
    /// Bildirimi sil (soft delete)
    /// </summary>
    public void Delete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        SetModifiedDate();

        AddDomainEvent(new NotificationDeletedEvent(Id, UserId));
    }

    /// <summary>
    /// Bildirimi geri al
    /// </summary>
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        SetModifiedDate();
    }

    /// <summary>
    /// Gönderildi olarak işaretle
    /// </summary>
    public void MarkAsSent(string channel = "InApp")
    {
        if (WasSent)
            return;

        WasSent = true;
        SentAt = DateTime.UtcNow;
        
        // Data'ya gönderim kanalını ekle
        AddData("sentChannel", channel);
        SetModifiedDate();
    }

    /// <summary>
    /// Ek veri ekle
    /// </summary>
    public void AddData(string key, object value)
    {
        Dictionary<string, object> dataDict;
        
        if (string.IsNullOrWhiteSpace(Data))
        {
            dataDict = new Dictionary<string, object>();
        }
        else
        {
            try
            {
                dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(Data) 
                    ?? new Dictionary<string, object>();
            }
            catch
            {
                dataDict = new Dictionary<string, object>();
            }
        }

        dataDict[key] = value;
        
        Data = JsonSerializer.Serialize(dataDict, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        SetModifiedDate();
    }

    /// <summary>
    /// Süre dolmuş mu kontrol et
    /// </summary>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }

    /// <summary>
    /// Bildirimin görünür olup olmadığını kontrol et
    /// </summary>
    public bool IsVisible()
    {
        return !IsDeleted && !IsExpired();
    }

    /// <summary>
    /// Bildirim özetini döndür
    /// </summary>
    public string? GetSummary()
    {
        var summary = Title;
        
        if (Priority == NotificationPriority.Critical)
            summary = "🚨 " + summary;
        else if (Priority == NotificationPriority.High)
            summary = "⚠️ " + summary;
            
        if (RequiresAction && !IsRead)
            summary += " [Aksiyon Gerekli]";
            
        return summary;
    }

    /// <summary>
    /// Bildirim yaşını hesapla
    /// </summary>
    public string GetAge()
    {
        var age = DateTime.UtcNow - CreatedAt;

        if (age.TotalMinutes < 1)
            return "Şimdi";
        if (age.TotalMinutes < 60)
            return $"{(int)age.TotalMinutes} dakika önce";
        if (age.TotalHours < 24)
            return $"{(int)age.TotalHours} saat önce";
        if (age.TotalDays < 7)
            return $"{(int)age.TotalDays} gün önce";
        if (age.TotalDays < 30)
            return $"{(int)(age.TotalDays / 7)} hafta önce";
        if (age.TotalDays < 365)
            return $"{(int)(age.TotalDays / 30)} ay önce";
        
        return $"{(int)(age.TotalDays / 365)} yıl önce";
    }

    // Private methods
    private void SetDefaultsByType()
    {
        IconType = Type switch
        {
            var t when t != null && t.StartsWith("Account.") => "user",
            var t when t != null && t.StartsWith("Review.") => "message-square",
            var t when t != null && t.StartsWith("Document.") => "file-check",
            var t when t != null && t.StartsWith("Moderation.") => "shield",
            var t when t != null && t.StartsWith("Badge.") => "award",
            var t when t != null && t.StartsWith("Company.") => "building",
            var t when t != null && t.StartsWith("System.") => "info",
            _ => "bell"
        };

        // Kritik bildirimler için süre
        if (Priority == NotificationPriority.Critical)
        {
            ExpiresAt = DateTime.UtcNow.AddDays(90); // 90 gün
        }
    }

    // Validation methods
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