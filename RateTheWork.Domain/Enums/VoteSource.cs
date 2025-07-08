namespace RateTheWork.Domain.Enums;

/// <summary>
/// Oy kaynağı - Oyun nereden verildiği
/// </summary>
public enum VoteSource
{
    /// <summary>
    /// Doğrudan yorum sayfasından
    /// </summary>
    Direct,
    
    /// <summary>
    /// Yorum listesinden
    /// </summary>
    ReviewList,
    
    /// <summary>
    /// Şirket profilinden
    /// </summary>
    CompanyProfile,
    
    /// <summary>
    /// Kullanıcı profilinden
    /// </summary>
    UserProfile,
    
    /// <summary>
    /// Arama sonuçlarından
    /// </summary>
    SearchResults
}

