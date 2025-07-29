namespace RateTheWork.Domain.Enums.Notification;

/// <summary>
/// Bildirim türleri
/// </summary>
public enum NotificationType
{
    // ========== Account Notifications ==========
    
    /// <summary>
    /// Hoş geldiniz bildirimi
    /// </summary>
    Welcome,
    
    /// <summary>
    /// Email doğrulandı
    /// </summary>
    EmailVerified,
    
    /// <summary>
    /// Telefon doğrulandı
    /// </summary>
    PhoneVerified,
    
    /// <summary>
    /// TC Kimlik doğrulandı
    /// </summary>
    TcIdentityVerified,
    
    /// <summary>
    /// Şifre değiştirildi
    /// </summary>
    PasswordChanged,
    
    /// <summary>
    /// Profil güncellendi
    /// </summary>
    ProfileUpdated,
    
    /// <summary>
    /// Hesap silindi
    /// </summary>
    AccountDeleted,
    
    // ========== Review Notifications ==========
    
    /// <summary>
    /// Yorumunuz onaylandı
    /// </summary>
    ReviewApproved,
    
    /// <summary>
    /// Yorumunuz reddedildi
    /// </summary>
    ReviewRejected,
    
    /// <summary>
    /// Yorumunuz oy aldı
    /// </summary>
    ReviewReceivedVote,
    
    ReviewReceivesVote = ReviewReceivedVote, // Alias olarak da ekle
    
    /// <summary>
    /// Yorumunuz yanıt aldı
    /// </summary>
    ReviewReceivedReply,
    
    /// <summary>
    /// Yorumunuz şikayet edildi
    /// </summary>
    ReviewReported,
    
    /// <summary>
    /// Yorumunuz gizlendi
    /// </summary>
    ReviewHidden,
    
    /// <summary>
    /// Yorumunuz tekrar aktif edildi
    /// </summary>
    ReviewActivated,
    
    
    
    // ========== Document Notifications ==========
    
    /// <summary>
    /// Belgeniz doğrulandı
    /// </summary>
    DocumentVerified,
    
    /// <summary>
    /// Belgeniz reddedildi
    /// </summary>
    DocumentRejected,
    
    /// <summary>
    /// Belge yükleme hatırlatması
    /// </summary>
    DocumentUploadReminder,
    
    // ========== Moderation Notifications ==========
    
    /// <summary>
    /// Uyarı aldınız
    /// </summary>
    WarningIssued,
    
    /// <summary>
    /// Hesabınız askıya alındı (ban)
    /// </summary>
    BanIssued,
    
    /// <summary>
    /// Hesap askısı kaldırıldı
    /// </summary>
    BanLifted,
    
    /// <summary>
    /// İtirazınız değerlendirildi
    /// </summary>
    AppealProcessed,
    
    // ========== Badge Notifications ==========
    
    /// <summary>
    /// Yeni rozet kazandınız
    /// </summary>
    BadgeEarned,
    
    /// <summary>
    /// Rozet kaldırıldı
    /// </summary>
    BadgeRemoved,
    
    /// <summary>
    /// Rozet seviyesi yükseldi
    /// </summary>
    BadgeLevelUp,
    
    // ========== Company Notifications ==========
    
    /// <summary>
    /// Şirket yorumunuza yanıt verdi
    /// </summary>
    CompanyResponded,
    
    /// <summary>
    /// Şirket doğrulandı
    /// </summary>
    CompanyVerified,
    
    /// <summary>
    /// Şirket durumu değişti
    /// </summary>
    CompanyStatusChanged,
    
    /// <summary>
    /// Takip ettiğiniz şirkete yeni yorum
    /// </summary>
    CompanyNewReview,
    
    /// <summary>
    /// Yeni yorum yapıldı
    /// </summary>
    NewReview,
    
    // ========== CV Application Notifications ==========
    
    /// <summary>
    /// CV'niz görüntülendi
    /// </summary>
    CVViewed,
    
    /// <summary>
    /// CV'niz indirildi
    /// </summary>
    CVDownloaded,
    
    /// <summary>
    /// CV başvurunuza yanıt verildi
    /// </summary>
    CVResponded,
    
    /// <summary>
    /// CV başvurunuzun süresi dolmak üzere
    /// </summary>
    CVExpiryWarning,
    
    /// <summary>
    /// CV başvurunuzun süresi doldu
    /// </summary>
    CVExpired,
    
    /// <summary>
    /// CV geri bildirim süresi uyarısı (şirket için)
    /// </summary>
    CVFeedbackDeadlineWarning,
    
    /// <summary>
    /// CV geri bildirim süresi doldu (şirket için)
    /// </summary>
    CVFeedbackOverdue,
    
    // ========== System Notifications ==========
    
    /// <summary>
    /// Sistem duyurusu
    /// </summary>
    SystemAnnouncement,
    
    /// <summary>
    /// Bakım bildirimi
    /// </summary>
    SystemMaintenance,
    
    /// <summary>
    /// Kullanım şartları güncellendi
    /// </summary>
    TermsUpdated,
    
    /// <summary>
    /// Gizlilik politikası güncellendi
    /// </summary>
    PrivacyPolicyUpdated,
    
    /// <summary>
    /// Yeni özellik duyurusu
    /// </summary>
    NewFeatureAnnouncement,
    
    // ========== Reminder Notifications ==========
    
    /// <summary>
    /// Profil tamamlama hatırlatması
    /// </summary>
    CompleteProfileReminder,
    
    /// <summary>
    /// Email doğrulama hatırlatması
    /// </summary>
    VerifyEmailReminder,
    
    /// <summary>
    /// Yorum yazma hatırlatması
    /// </summary>
    WriteReviewReminder,
    
    // ========== Social Notifications ==========
    
    /// <summary>
    /// Birisi sizi takip etti
    /// </summary>
    NewFollower,
    
    /// <summary>
    /// Takip ettiğiniz kişi yorum yaptı
    /// </summary>
    FollowingUserReviewed,
    
    // ========== Other ==========
    
    /// <summary>
    /// Diğer bildirimler
    /// </summary>
    Other
}

// Extension Methods for NotificationType
public static class NotificationTypeExtensions
{
    /// <summary>
    /// Bildirim türüne göre varsayılan başlık döner
    /// </summary>
    public static string GetDefaultTitle(this NotificationType type)
    {
        return type switch
        {
            NotificationType.Welcome => "RateTheWork'e Hoş Geldiniz! 🎉",
            NotificationType.EmailVerified => "Email Adresiniz Doğrulandı ✅",
            NotificationType.PhoneVerified => "Telefon Numaranız Doğrulandı ✅",
            NotificationType.ReviewApproved => "Yorumunuz Onaylandı! 👍",
            NotificationType.ReviewRejected => "Yorumunuz Reddedildi 😔",
            NotificationType.ReviewReceivedVote => "Yorumunuz Oy Aldı! 🗳️",
            NotificationType.WarningIssued => "Uyarı! ⚠️",
            NotificationType.BanIssued => "Hesabınız Askıya Alındı 🚫",
            NotificationType.BadgeEarned => "Yeni Rozet Kazandınız! 🏆",
            NotificationType.CompanyResponded => "Şirket Yanıtladı 💬",
            NotificationType.SystemAnnouncement => "Sistem Duyurusu 📢",
            _ => "Bildirim"
        };
    }
    
    /// <summary>
    /// Bildirim türüne göre varsayılan öncelik döner
    /// </summary>
    public static NotificationPriority GetDefaultPriority(this NotificationType type)
    {
        return type switch
        {
            NotificationType.BanIssued => NotificationPriority.Critical,
            NotificationType.WarningIssued => NotificationPriority.High,
            NotificationType.SystemMaintenance => NotificationPriority.High,
            NotificationType.TermsUpdated => NotificationPriority.High,
            NotificationType.ReviewRejected => NotificationPriority.High,
            NotificationType.BadgeEarned => NotificationPriority.Low,
            NotificationType.NewFollower => NotificationPriority.Low,
            _ => NotificationPriority.Normal
        };
    }
    
    /// <summary>
    /// Bildirim türüne göre varsayılan kanalları döner
    /// </summary>
    public static NotificationChannel GetDefaultChannels(this NotificationType type)
    {
        return type switch
        {
            // Kritik bildirimler tüm kanallardan
            NotificationType.BanIssued => NotificationChannel.All,
            NotificationType.WarningIssued => NotificationChannel.All,
            NotificationType.PasswordChanged => NotificationChannel.All,
            
            // Önemli bildirimler email + in-app
            NotificationType.ReviewApproved => NotificationChannel.InApp | NotificationChannel.Email,
            NotificationType.ReviewRejected => NotificationChannel.InApp | NotificationChannel.Email,
            NotificationType.DocumentVerified => NotificationChannel.InApp | NotificationChannel.Email,
            NotificationType.DocumentRejected => NotificationChannel.InApp | NotificationChannel.Email,
            
            // Sosyal bildirimler sadece in-app
            NotificationType.ReviewReceivedVote => NotificationChannel.InApp,
            NotificationType.BadgeEarned => NotificationChannel.InApp,
            NotificationType.NewFollower => NotificationChannel.InApp,
            
            // Diğerleri varsayılan in-app
            _ => NotificationChannel.InApp
        };
    }
    
    /// <summary>
    /// Bildirim türünün email gönderilebilir olup olmadığını kontrol eder
    /// </summary>
    public static bool IsEmailEligible(this NotificationType type)
    {
        return GetDefaultChannels(type).HasFlag(NotificationChannel.Email);
    }
    
    /// <summary>
    /// Bildirim türünün SMS gönderilebilir olup olmadığını kontrol eder
    /// </summary>
    public static bool IsSmsEligible(this NotificationType type)
    {
        return GetDefaultChannels(type).HasFlag(NotificationChannel.Sms);
    }
}