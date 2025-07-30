using RateTheWork.Domain.Common;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Sistem raporu entity'si - Otomatik olarak oluşturulan raporları tutar
/// </summary>
public class SystemReport : BaseEntity
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private SystemReport() : base()
    {
    }

    // Properties
    public string Type { get; set; } = string.Empty; // MonthlyCompanyReport, WeeklySystemReport
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string FileUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    public Guid CreatedById { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Yeni sistem raporu oluşturur
    /// </summary>
    public static SystemReport Create
    (
        string type
        , string name
        , string description
        , Dictionary<string, object> parameters
        , Guid createdById
    )
    {
        var report = new SystemReport
        {
            Type = type, Name = name, Description = description, Parameters = parameters, CreatedById = createdById
            , Status = "Pending", GeneratedAt = DateTime.UtcNow
        };

        return report;
    }

    /// <summary>
    /// Raporu tamamlandı olarak işaretle
    /// </summary>
    public void MarkAsCompleted(string fileUrl)
    {
        Status = "Completed";
        FileUrl = fileUrl;
        GeneratedAt = DateTime.UtcNow;
        SetModifiedDate();
    }

    /// <summary>
    /// Raporu başarısız olarak işaretle
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        Status = "Failed";
        ErrorMessage = errorMessage;
        SetModifiedDate();
    }
}
