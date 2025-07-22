namespace RateTheWork.Domain.Enums.JobPosting;

/// <summary>
/// Deneyim seviyesi
/// </summary>
public enum ExperienceLevel
{
    /// <summary>
    /// Deneyim gerekmiyor
    /// </summary>
    NoExperience

    ,

    /// <summary>
    /// Giriş seviyesi (0-2 yıl)
    /// </summary>
    EntryLevel

    ,

    /// <summary>
    /// Orta seviye (2-5 yıl)
    /// </summary>
    MidLevel

    ,

    /// <summary>
    /// Kıdemli (5-8 yıl)
    /// </summary>
    Senior

    ,

    /// <summary>
    /// Uzman (8+ yıl)
    /// </summary>
    Expert

    ,

    /// <summary>
    /// Yönetici seviyesi
    /// </summary>
    Manager

    ,

    /// <summary>
    /// Direktör seviyesi
    /// </summary>
    Director

    ,

    /// <summary>
    /// C-Level (CEO, CTO, vb.)
    /// </summary>
    Executive
}
