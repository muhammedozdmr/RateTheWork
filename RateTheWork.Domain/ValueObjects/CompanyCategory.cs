namespace RateTheWork.Domain.ValueObjects;

/// <summary>
/// Şirket kategorisi
/// </summary>
public class CompanyCategory
{
    public string MainCategory { get; set; } = string.Empty;
    public List<string> SubCategories { get; set; } = new();
    public string CompanySize { get; set; } = string.Empty; // Micro, Small, Medium, Large, Enterprise
    public string MaturityLevel { get; set; } = string.Empty; // Startup, Growing, Mature, Declining
}
