using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

/// <summary>
/// Rozet sistemi için repository implementasyonu
/// </summary>
public class BadgeRepository : BaseRepository<Badge>, IBadgeRepository
{
    public BadgeRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Rozet koduna göre rozet getirir
    /// </summary>
    public async Task<Badge?> GetByCodeAsync(string badgeCode)
    {
        return await _context.Badges
            .FirstOrDefaultAsync(b => b.Code == badgeCode);
    }

    /// <summary>
    /// Aktif rozetleri getirir
    /// </summary>
    public async Task<IReadOnlyList<Badge>> GetActiveBadgesAsync()
    {
        var badges = await _context.Badges
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();

        return badges.AsReadOnly();
    }

    /// <summary>
    /// Belirli kategorideki rozetleri getirir
    /// </summary>
    public async Task<IReadOnlyList<Badge>> GetBadgesByCategoryAsync(string category)
    {
        var badges = await _context.Badges
            .Where(b => b.Category == category && b.IsActive)
            .OrderBy(b => b.SortOrder)
            .ThenBy(b => b.Name)
            .ToListAsync();

        return badges.AsReadOnly();
    }

    /// <summary>
    /// Otomatik olarak verilebilecek rozetleri getirir
    /// </summary>
    public async Task<IReadOnlyList<Badge>> GetAutoAwardableBadgesAsync()
    {
        var badges = await _context.Badges
            .Where(b => b.IsActive && b.IsAutoAwardable)
            .OrderBy(b => b.Level)
            .ThenBy(b => b.SortOrder)
            .ToListAsync();

        return badges.AsReadOnly();
    }

    /// <summary>
    /// Belirli seviyedeki rozetleri getirir
    /// </summary>
    public async Task<IReadOnlyList<Badge>> GetBadgesByLevelAsync(int level)
    {
        var badges = await _context.Badges
            .Where(b => b.Level == level && b.IsActive)
            .OrderBy(b => b.SortOrder)
            .ThenBy(b => b.Name)
            .ToListAsync();

        return badges.AsReadOnly();
    }

    /// <summary>
    /// Kullanıcının kazandığı rozetleri getirir
    /// </summary>
    public async Task<IEnumerable<Badge>> GetUserBadgesAsync(string userId)
    {
        return await _context.Badges
            .Where(b => b.UserBadges.Any(ub => ub.UserId == userId))
            .OrderByDescending(b => b.UserBadges.First(ub => ub.UserId == userId).AwardedAt)
            .ToListAsync();
    }
}
