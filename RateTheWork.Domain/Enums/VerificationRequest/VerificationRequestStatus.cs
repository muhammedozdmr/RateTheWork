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
    /// İşleniyor
    /// </summary>
    Processing = 1

    ,

    /// <summary>
    /// Onaylandı
    /// </summary>
    Approved = 2

    ,

    /// <summary>
    /// Reddedildi
    /// </summary>
    Rejected = 3

    ,

    /// <summary>
    /// İptal edildi
    /// </summary>
    Cancelled = 4
}
