namespace RateTheWork.Domain.Enums;

/// <summary>
/// Bildirim türleri
/// </summary>
public static class NotificationTypes
{
    public const string Welcome = "Hoş Geldiniz";
    public const string EmailVerification = "Email Doğrulama";
    public const string ReviewApproved = "Yorumunuz Onaylandı";
    public const string ReviewRejected = "Yorumunuz Reddedildi";
    public const string DocumentVerified = "Belgeniz Doğrulandı";
    public const string WarningIssued = "Uyarı";
    public const string BadgeEarned = "Rozet Kazandınız";
    public const string ReviewReply = "Yorumunuza Yanıt";
}
