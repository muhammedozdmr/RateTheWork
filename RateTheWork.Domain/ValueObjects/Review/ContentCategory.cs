namespace RateTheWork.Domain.ValueObjects.Review;

/// <summary>
/// İçerik kategorisi
/// </summary>
public class ContentCategory
{
    public string CategoryName { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<string> SubCategories { get; set; } = new();
}
