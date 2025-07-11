namespace RateTheWork.Domain.Enums.Notification;

/// <summary>
/// Bildirim kanalları - Flags enum olarak tanımlanmıştır, birden fazla kanal seçilebilir
/// </summary>
[Flags]
public enum NotificationChannel
{
    /// <summary>
    /// Hiçbir kanal seçili değil
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Uygulama içi bildirim
    /// </summary>
    InApp = 1,
    
    /// <summary>
    /// Email bildirimi
    /// </summary>
    Email = 2,
    
    /// <summary>
    /// SMS bildirimi
    /// </summary>
    Sms = 4,
    
    /// <summary>
    /// Push notification (mobil)
    /// </summary>
    Push = 8,
    
    /// <summary>
    /// Tüm kanallar
    /// </summary>
    All = InApp | Email | Sms | Push
}

// Kullanım örnekleri:
// Sadece uygulama içi: NotificationChannel.InApp
// Email ve SMS: NotificationChannel.Email | NotificationChannel.SMS
// Tüm kanallar: NotificationChannel.All
// Kontrol: if (channels.HasFlag(NotificationChannel.Email)) { ... }
