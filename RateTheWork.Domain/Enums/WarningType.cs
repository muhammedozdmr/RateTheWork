namespace RateTheWork.Domain.Enums;

/// <summary>
/// Uyarı türleri
/// </summary>
public enum WarningType
{
    /// <summary>
    /// İçerik ihlali
    /// </summary>
    ContentViolation,
    
    /// <summary>
    /// Spam davranışı
    /// </summary>
    SpamBehavior,
    
    /// <summary>
    /// Yanlış bilgi
    /// </summary>
    FalseInformation,
    
    /// <summary>
    /// Saygısız davranış
    /// </summary>
    DisrespectfulBehavior,
    
    /// <summary>
    /// Diğer
    /// </summary>
    Other
}

