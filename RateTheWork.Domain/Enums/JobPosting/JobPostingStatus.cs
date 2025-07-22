namespace RateTheWork.Domain.Enums.JobPosting;

/// <summary>
/// İş ilanı durumu
/// </summary>
public enum JobPostingStatus
{
    /// <summary>
    /// Taslak
    /// </summary>
    Draft

    ,

    /// <summary>
    /// Onay bekliyor
    /// </summary>
    PendingApproval

    ,

    /// <summary>
    /// Aktif
    /// </summary>
    Active

    ,

    /// <summary>
    /// Durduruldu
    /// </summary>
    Paused

    ,

    /// <summary>
    /// Süresi doldu
    /// </summary>
    Expired

    ,

    /// <summary>
    /// Tamamlandı
    /// </summary>
    Completed

    ,

    /// <summary>
    /// İptal edildi
    /// </summary>
    Cancelled

    ,

    /// <summary>
    /// Reddedildi
    /// </summary>
    Rejected
}
