namespace RateTheWork.Domain.Enums.User;

/// <summary>
/// Ban türleri
/// </summary>
public enum BanType
{
    /// <summary>
    /// Geçici ban
    /// </summary>
    Temporary,
    
    /// <summary>
    /// Kalıcı ban
    /// </summary>
    Permanent,
    
    /// <summary>
    /// Sistem otomatik banı
    /// </summary>
    SystemAutomatic
}
