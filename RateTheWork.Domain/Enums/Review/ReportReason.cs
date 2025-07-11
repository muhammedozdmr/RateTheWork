namespace RateTheWork.Domain.Enums.Review;

/// <summary>
/// Şikayet nedenleri
/// </summary>
public enum ReportReason
{
    /// <summary>
    /// Uygunsuz içerik
    /// </summary>
    InappropriateContent,
    
    /// <summary>
    /// Yanlış veya yanıltıcı bilgi
    /// </summary>
    FalseInformation,
    
    /// <summary>
    /// Spam
    /// </summary>
    Spam,
    
    /// <summary>
    /// Taciz veya zorbalık
    /// </summary>
    Harassment,
    
    /// <summary>
    /// Kişisel saldırı
    /// </summary>
    PersonalAttack,
    
    /// <summary>
    /// Gizli bilgi paylaşımı
    /// </summary>
    ConfidentialInfo,
    
    /// <summary>
    /// Konu dışı içerik
    /// </summary>
    OffTopic,
    
    /// <summary>
    /// Mükerrer içerik
    /// </summary>
    Duplicate,
    
    /// <summary>
    /// Diğer
    /// </summary>
    Other
}
