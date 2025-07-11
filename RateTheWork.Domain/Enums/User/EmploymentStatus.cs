namespace RateTheWork.Domain.Enums;

/// <summary>
/// Çalışma durumu
/// </summary>
public enum EmploymentStatus
{
    /// <summary>
    /// Mevcut çalışan
    /// </summary>
    CurrentEmployee,
    
    /// <summary>
    /// Eski çalışan
    /// </summary>
    FormerEmployee,
    
    /// <summary>
    /// Stajyer
    /// </summary>
    Intern,
    
    /// <summary>
    /// Sözleşmeli/Freelance
    /// </summary>
    Contractor,
    
    /// <summary>
    /// Yarı zamanlı
    /// </summary>
    PartTime,
    
    /// <summary>
    /// Tam zamanlı
    /// </summary>
    FullTime
}
