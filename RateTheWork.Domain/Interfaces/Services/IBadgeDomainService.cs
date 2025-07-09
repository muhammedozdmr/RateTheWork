using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Services;

/// <summary>
/// Rozet işlemleri için domain service interface'i
/// </summary>
public interface IBadgeDomainService
{
    /// <summary>
    /// Kullanıcının kazanabileceği rozetleri kontrol eder
    /// </summary>
    Task<List<Badge>> CheckEligibleBadgesAsync(string userId);

    /// <summary>
    /// Kullanıcıya rozet atar
    /// </summary>
    Task AwardBadgeAsync(string userId, string badgeId);

    /// <summary>
    /// Rozet kriterlerini kontrol eder
    /// </summary>
    Task<bool> CheckBadgeCriteriaAsync(string userId, Badge badge);
    
    /// <summary>
    /// Kullanıcının rozet ilerlemesini hesaplar
    /// </summary>
    Task<Dictionary<string, BadgeProgress>> GetUserBadgeProgressAsync(string userId);
    
    /// <summary>
    /// Rozet puanlarını hesaplar
    /// </summary>
    Task<int> CalculateUserBadgePointsAsync(string userId);
}
