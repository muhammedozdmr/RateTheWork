namespace RateTheWork.Domain.Exceptions;

/// <summary>
/// Exception severity levels
/// </summary>
public enum ExceptionSeverity
{
    /// <summary>
    /// Low severity - minor issues that don't affect core functionality
    /// </summary>
    Low = 1

    ,

    /// <summary>
    /// Medium severity - issues that affect user experience but system continues
    /// </summary>
    Medium = 2

    ,

    /// <summary>
    /// High severity - significant issues that require immediate attention
    /// </summary>
    High = 3

    ,

    /// <summary>
    /// Critical severity - system-breaking issues that require urgent intervention
    /// </summary>
    Critical = 4
}
