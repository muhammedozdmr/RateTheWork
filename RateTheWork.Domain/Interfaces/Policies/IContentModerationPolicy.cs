namespace RateTheWork.Domain.Interfaces.Policies;

/// <summary>
/// İçerik moderasyon policy interface'i
/// </summary>
public interface IContentModerationPolicy : IDomainPolicy
{
    /// <summary>
    /// İçerik uygun mu?
    /// </summary>
    Task<(bool IsAppropriate, string[] Issues)> CheckContentAsync(string content);
    
    /// <summary>
    /// Otomatik red kriterleri
    /// </summary>
    string[] GetAutoRejectKeywords();
    
    /// <summary>
    /// Manuel inceleme gerektiren kriterler
    /// </summary>
    string[] GetManualReviewTriggers();
    
    /// <summary>
    /// Spam skoru hesaplama
    /// </summary>
    Task<decimal> CalculateSpamScoreAsync(string content, string userId);
}
