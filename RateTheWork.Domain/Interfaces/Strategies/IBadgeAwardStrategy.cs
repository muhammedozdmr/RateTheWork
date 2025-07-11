using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Strategies;

/// <summary>
/// Rozet kazanma strategy interface'i
/// </summary>
public interface IBadgeAwardStrategy
{
    /// <summary>
    /// Rozet kazanma kriterleri kontrolü
    /// </summary>
    Task<bool> CheckEligibilityAsync(string userId, Badge badge);
    
    /// <summary>
    /// İlerleme yüzdesi hesaplama
    /// </summary>
    Task<decimal> CalculateProgressAsync(string userId, Badge badge);
    
    /// <summary>
    /// Sonraki rozet için gereksinimler
    /// </summary>
    Task<Dictionary<string, object>> GetRequirementsForNextLevelAsync(string userId, string badgeType);
}
