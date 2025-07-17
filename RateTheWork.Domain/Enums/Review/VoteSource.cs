namespace RateTheWork.Domain.Enums.Review;

/// <summary>
/// Oy kaynağı - Oyun nereden verildiği
/// </summary>
public enum VoteSource
{
    /// <summary>
    /// Doğrudan yorum sayfasından
    /// </summary>
    Direct

    ,

    /// <summary>
    /// Yorum listesinden
    /// </summary>
    ReviewList

    ,

    /// <summary>
    /// Şirket profilinden
    /// </summary>
    CompanyProfile

    ,

    /// <summary>
    /// Kullanıcı profilinden
    /// </summary>
    UserProfile

    ,

    /// <summary>
    /// Arama sonuçlarından
    /// </summary>
    SearchResults

    ,

    /// <summary>
    /// Web uygulamasından
    /// </summary>
    Web

    ,

    /// <summary>
    /// Mobil uygulamadan
    /// </summary>
    Mobile

    ,

    /// <summary>
    /// API üzerinden
    /// </summary>
    Api
}
