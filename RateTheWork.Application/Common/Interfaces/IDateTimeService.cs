namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Tarih/saat işlemleri için abstraction.
/// Test edilebilirlik için DateTime.Now yerine bu servis kullanılır.
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Şu anki UTC zamanı
    /// </summary>
    DateTime UtcNow { get; }
    
    /// <summary>
    /// Şu anki yerel zaman (Türkiye saati)
    /// </summary>
    DateTime Now { get; }
    
    /// <summary>
    /// Bugünün tarihi (saat kısmı 00:00:00)
    /// </summary>
    DateTime Today { get; }
}
