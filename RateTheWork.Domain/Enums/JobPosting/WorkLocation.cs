namespace RateTheWork.Domain.Enums.JobPosting;

/// <summary>
/// Çalışma lokasyonu
/// </summary>
public enum WorkLocation
{
    /// <summary>
    /// Ofiste çalışma
    /// </summary>
    OnSite = 1

    ,

    /// <summary>
    /// Uzaktan çalışma
    /// </summary>
    Remote = 2

    ,

    /// <summary>
    /// Hibrit çalışma (ofis + uzaktan)
    /// </summary>
    Hybrid = 3
}
