namespace RateTheWork.Domain.Enums.VerificationRequest;

/// <summary>
/// Doğrulama isteği durumları
/// </summary>
public enum VerificationRequestStatus
{
    /// <summary>
    /// Beklemede
    /// </summary>
    Pending = 0

    ,

    /// <summary>
    /// Onaylandı
    /// </summary>
    Approved = 1

    ,

    /// <summary>
    /// Reddedildi
    /// </summary>
    Rejected = 2

    ,

    /// <summary>
    /// İptal edildi
    /// </summary>
    Cancelled = 3
}
