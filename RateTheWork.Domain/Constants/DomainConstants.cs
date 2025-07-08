namespace RateTheWork.Domain.Constants;

/// <summary>
/// Domain katmanı sabitleri
/// </summary>
public static class DomainConstants
{
    /// <summary>
    /// Yorum ile ilgili sabitler
    /// </summary>
    public static class Review
    {
        public const int MinCommentLength = 50;
        public const int MaxCommentLength = 2000;
        public const decimal MinRating = 1.0m;
        public const decimal MaxRating = 5.0m;
        public const decimal RatingStep = 0.5m;
        public const int MaxReportCountBeforeAutoHide = 5;
        public const int MaxEditHours = 24;
        public const int MaxEditCount = 3;
        public const int ReviewCooldownDays = 365; // Aynı şirkete aynı tip yorum için bekleme süresi
        public const int MinCharactersForDetailedReviewer = 500; // Detaylı yorumcu rozeti için
    }

    /// <summary>
    /// Kullanıcı ile ilgili sabitler
    /// </summary>
    public static class User
    {
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;
        public const int MinUsernameLength = 3;
        public const int MaxUsernameLength = 50;
        public const int MaxWarningsBeforeAutoBan = 3;
        public const int EmailVerificationTokenExpiryHours = 24;
        public const int PhoneVerificationCodeExpiryMinutes = 10;
        public const int PhoneVerificationCodeLength = 6;
        public const int RefreshTokenExpiryDays = 30;
        public const int PasswordResetTokenExpiryHours = 24;
    }

    /// <summary>
    /// Şirket ile ilgili sabitler
    /// </summary>
    public static class Company
    {
        public const int MaxNameLength = 200;
        public const int MaxAddressLength = 500;
        public const int TaxIdLength = 10;
        public const int MersisNoLength = 16;
        public const int MaxSectorLength = 100;
        public const int MinEmployeeCount = 1;
        public const int MaxWebsiteUrlLength = 255;
        public const int MaxSocialMediaUrlLength = 255;
    }

    /// <summary>
    /// Admin ile ilgili sabitler
    /// </summary>
    public static class Admin
    {
        public const int MinUsernameLength = 3;
        public const int MaxUsernameLength = 50;
        public const int PasswordResetTokenExpiryHours = 24;
        public const int TwoFactorCodeLength = 6;
        public const int TwoFactorCodeExpiryMinutes = 5;
        public const int SessionTimeoutMinutes = 30;
    }

    /// <summary>
    /// Rozet ile ilgili sabitler
    /// </summary>
    public static class Badge
    {
        public const int FirstReviewThreshold = 1;
        public const int ActiveReviewerThreshold = 10;
        public const int TrustedReviewerThreshold = 5; // Doğrulanmış yorum sayısı
        public const int TopContributorThreshold = 50;
        public const int CompanyExplorerThreshold = 10; // Farklı şirket sayısı
        public const int HelpfulReviewerPercentage = 80; // Upvote oranı
        public const int MaxBadgeNameLength = 100;
        public const int MaxBadgeDescriptionLength = 500;
        public const int DefaultBadgePoints = 10;
    }

    /// <summary>
    /// Güvenlik ile ilgili sabitler
    /// </summary>
    public static class Security
    {
        public const int MaxFailedLoginAttempts = 5;
        public const int AccountLockMinutes = 30;
        public const int TokenExpiryDays = 7;
        public const int RefreshTokenExpiryDays = 30;
        public const int SessionTimeoutMinutes = 20;
        public const int IpAddressTrackingDays = 90; // KVKK uyumluluğu
        public const int AuditLogRetentionDays = 365;
    }

    /// <summary>
    /// Bildirim ile ilgili sabitler
    /// </summary>
    public static class Notification
    {
        public const int MaxTitleLength = 100;
        public const int MaxMessageLength = 500;
        public const int DefaultExpirationDays = 30;
        public const int BulkNotificationBatchSize = 100;
        public const int MaxRetryCount = 3;
        public const int RetryDelayMinutes = 5;
    }

    /// <summary>
    /// Doğrulama ile ilgili sabitler
    /// </summary>
    public static class Verification
    {
        public const int StandardProcessingHours = 24;
        public const int UrgentProcessingHours = 2;
        public const int DocumentNameMaxLength = 255;
        public const int ProcessingNotesMaxLength = 1000;
        public const int AppealDeadlineDays = 7;
        public const int ResubmissionLimitDays = 30;
        public const int MaxResubmissionCount = 3;
    }

    /// <summary>
    /// Uyarı ve Ban ile ilgili sabitler
    /// </summary>
    public static class Moderation
    {
        // Warning
        public const int WarningReasonMinLength = 10;
        public const int WarningReasonMaxLength = 500;
        public const int LowSeverityPoints = 1;
        public const int MediumSeverityPoints = 2;
        public const int HighSeverityPoints = 3;
        public const int CriticalSeverityPoints = 5;
        public const int DefaultWarningExpirationDays = 90;
        
        // Ban
        public const int BanReasonMinLength = 5;
        public const int MaxTemporaryBanDays = 365;
        public const int AutomaticBanDays = 30;
        public const int BanAppealDeadlineDays = 7;
        public const int PermanentBanAppealDeadlineDays = 30;
    }

    /// <summary>
    /// Şikayet ile ilgili sabitler
    /// </summary>
    public static class Report
    {
        public const int ReportDetailsMaxLength = 1000;
        public const int ActionTakenMinLength = 10;
        public const int AdminNotesMaxLength = 500;
        public const int EscalationThresholdHours = 48;
        public const int AutoEscalationReportCount = 10;
    }

    /// <summary>
    /// Sayfalama ile ilgili sabitler
    /// </summary>
    public static class Pagination
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        public const int DefaultPageNumber = 1;
    }

    /// <summary>
    /// Cache ile ilgili sabitler
    /// </summary>
    public static class Cache
    {
        public const int ShortCacheDurationMinutes = 5;
        public const int MediumCacheDurationMinutes = 30;
        public const int LongCacheDurationMinutes = 120;
        public const int DailyCacheDurationHours = 24;
    }

    /// <summary>
    /// Rate Limiting ile ilgili sabitler
    /// </summary>
    public static class RateLimit
    {
        public const int LoginAttemptsPerHour = 10;
        public const int ReviewsPerDay = 5;
        public const int ReportsPerDay = 10;
        public const int ApiCallsPerMinute = 60;
        public const int PasswordResetRequestsPerDay = 3;
    }
}
