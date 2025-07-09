namespace RateTheWork.Domain.Interfaces.Policies;

/// <summary>
/// Review oluşturma policy interface'i
/// </summary>
public interface IReviewCreationPolicy : IDomainPolicy
{
    /// <summary>
    /// Kullanıcı review oluşturabilir mi?
    /// </summary>
    Task<(bool CanCreate, string? Reason)> CanCreateReviewAsync(
        string userId, 
        string companyId, 
        string reviewType);
    
    /// <summary>
    /// Minimum bekleme süresi (aynı şirkete tekrar review için)
    /// </summary>
    TimeSpan GetMinimumWaitPeriod(string reviewType);
    
    /// <summary>
    /// Maksimum review sayısı (belirli bir period için)
    /// </summary>
    int GetMaxReviewsPerPeriod(TimeSpan period);
}
