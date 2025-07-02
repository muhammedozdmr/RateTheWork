namespace RateTheWork.Domain.Enums;

/// <summary>
/// Şikayet durumları
/// </summary>
public static class ReportStatuses
{
    public const string Pending = "Beklemede";
    public const string UnderReview = "İnceleniyor";
    public const string ActionTaken = "İşlem Yapıldı";
    public const string Dismissed = "Reddedildi";
}
