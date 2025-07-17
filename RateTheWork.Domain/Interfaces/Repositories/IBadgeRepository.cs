using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Rozet repository interface'i
/// </summary>
public interface IBadgeRepository : IRepository<Badge>
{
    /// <summary>
    /// Aktif rozetleri getirir
    /// </summary>
    Task<IReadOnlyList<Badge>> GetActiveBadgesAsync();

    /// <summary>
    /// Belirli kategorideki rozetleri getirir
    /// </summary>
    Task<IReadOnlyList<Badge>> GetBadgesByCategoryAsync(string category);

    /// <summary>
    /// Otomatik olarak verilebilecek rozetleri getirir
    /// </summary>
    Task<IReadOnlyList<Badge>> GetAutoAwardableBadgesAsync();

    /// <summary>
    /// Rozet koduna göre rozet getirir
    /// </summary>
    Task<Badge?> GetByCodeAsync(string badgeCode);

    /// <summary>
    /// Belirli seviyedeki rozetleri getirir
    /// </summary>
    Task<IReadOnlyList<Badge>> GetBadgesByLevelAsync(int level);
}
