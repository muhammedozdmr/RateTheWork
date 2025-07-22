namespace RateTheWork.Domain.Enums.Subscription;

/// <summary>
/// Fatura döngüsü
/// </summary>
public enum BillingCycle
{
    /// <summary>
    /// Aylık ödeme
    /// </summary>
    Monthly

    ,

    /// <summary>
    /// 3 aylık ödeme
    /// </summary>
    Quarterly

    ,

    /// <summary>
    /// 6 aylık ödeme
    /// </summary>
    SemiAnnual

    ,

    /// <summary>
    /// Yıllık ödeme
    /// </summary>
    Annual

    ,

    /// <summary>
    /// Tek seferlik ödeme
    /// </summary>
    OneTime
}
