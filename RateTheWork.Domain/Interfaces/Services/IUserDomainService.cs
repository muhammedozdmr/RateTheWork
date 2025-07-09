namespace RateTheWork.Domain.Interfaces.Services;

/// <summary>
/// Kullanıcı işlemleri için domain service interface'i
/// </summary>
public interface IUserDomainService
{
    /// <summary>
    /// Kullanıcı aktivitesini kontrol eder
    /// </summary>
    Task<bool> IsUserActiveAsync(string userId);
    
    /// <summary>
    /// Kullanıcının yorum yapma yetkisini kontrol eder
    /// </summary>
    Task<bool> CanUserCreateReviewAsync(string userId);
    
    /// <summary>
    /// Kullanıcı güvenilirlik skorunu hesaplar
    /// </summary>
    Task<decimal> CalculateUserReliabilityScoreAsync(string userId);
    
    /// <summary>
    /// Kullanıcı anonimlik seviyesini belirler
    /// </summary>
    Task<string> DetermineAnonymityLevelAsync(string userId, string companyId);
    
    /// <summary>
    /// Kullanıcının şirkette çalışıp çalışmadığını doğrular
    /// </summary>
    Task<bool> VerifyEmploymentAsync(string userId, string companyId, string verificationMethod);
    
    /// <summary>
    /// Kullanıcı aktivite özetini getirir
    /// </summary>
    Task<UserActivitySummary> GetUserActivitySummaryAsync(string userId, DateTime? startDate = null);
    
    /// <summary>
    /// Kullanıcı tercihlerini analiz eder
    /// </summary>
    Task<UserPreferences> AnalyzeUserPreferencesAsync(string userId);
    
    /// <summary>
    /// Kullanıcı için önerilen şirketleri getirir
    /// </summary>
    Task<List<Company>> GetRecommendedCompaniesAsync(string userId, int maxResults = 10);
    
    /// <summary>
    /// Kullanıcı davranış skorunu hesaplar
    /// </summary>
    Task<UserBehaviorScore> CalculateUserBehaviorScoreAsync(string userId);
}
