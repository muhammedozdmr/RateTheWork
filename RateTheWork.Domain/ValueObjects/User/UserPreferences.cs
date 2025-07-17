namespace RateTheWork.Domain.ValueObjects.User;

/// <summary>
/// Kullanıcı tercihleri
/// </summary>
public class UserPreferences
{
    public List<string> PreferredSectors { get; set; } = new();
    public List<string> PreferredCompanySizes { get; set; } = new();
    public List<string> PreferredCategories { get; set; } = new();
    public Dictionary<string, decimal> CategoryImportanceWeights { get; set; } = new();
    public string PreferredReviewStyle { get; set; } = string.Empty; // Detailed, Brief, Balanced
}
