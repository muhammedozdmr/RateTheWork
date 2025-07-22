namespace RateTheWork.Domain.Enums.JobApplication;

/// <summary>
/// İş başvurusu durumu
/// </summary>
public enum ApplicationStatus
{
    /// <summary>
    /// Başvuru alındı
    /// </summary>
    Received

    ,

    /// <summary>
    /// İnceleniyor
    /// </summary>
    UnderReview

    ,

    /// <summary>
    /// Ön elemeye alındı
    /// </summary>
    Shortlisted

    ,

    /// <summary>
    /// Mülakat davet edildi
    /// </summary>
    InterviewInvited

    ,

    /// <summary>
    /// Mülakat yapıldı
    /// </summary>
    Interviewed

    ,

    /// <summary>
    /// Değerlendiriliyor
    /// </summary>
    UnderEvaluation

    ,

    /// <summary>
    /// Teklif yapıldı
    /// </summary>
    OfferMade

    ,

    /// <summary>
    /// Teklif kabul edildi
    /// </summary>
    OfferAccepted

    ,

    /// <summary>
    /// Teklif reddedildi
    /// </summary>
    OfferDeclined

    ,

    /// <summary>
    /// İşe alındı
    /// </summary>
    Hired

    ,

    /// <summary>
    /// Reddedildi
    /// </summary>
    Rejected

    ,

    /// <summary>
    /// Aday çekildi
    /// </summary>
    Withdrawn

    ,

    /// <summary>
    /// Yetenek havuzuna alındı
    /// </summary>
    TalentPool
}
