namespace RateTheWork.Domain.Enums;

/// <summary>
/// Onay durumları
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// Onay bekliyor
    /// </summary>
    Pending,
    
    /// <summary>
    /// İnceleme altında
    /// </summary>
    UnderReview,
    
    /// <summary>
    /// Onaylandı
    /// </summary>
    Approved,
    
    /// <summary>
    /// Reddedildi
    /// </summary>
    Rejected,
    
    /// <summary>
    /// Askıya alındı
    /// </summary>
    Suspended,
    
    /// <summary>
    /// Otomatik onaylandı
    /// </summary>
    AutoApproved
}
