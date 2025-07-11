namespace RateTheWork.Domain.Enums.Notification;

/// <summary>
/// Bildirim öncelik seviyesi
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// Düşük öncelik
    /// </summary>
    Low,
    
    /// <summary>
    /// Normal öncelik
    /// </summary>
    Normal,
    
    /// <summary>
    /// Yüksek öncelik
    /// </summary>
    High,
    
    /// <summary>
    /// Kritik öncelik
    /// </summary>
    Critical
}