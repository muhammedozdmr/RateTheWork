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
    public async Task<Badge?> GetByCodeAsync(string code)
    {
        return await _context.Badges
            .FirstOrDefaultAsync(b => b.Code == code);
    }

    /// <summary>
    /// Aktif rozetleri getirir
    /// </summary>
    public async Task<IEnumerable<Badge>> GetActiveBadgesAsync()
    {
        return await _context.Badges
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Kategori bazında rozetleri getirir
    /// </summary>
    public async Task<IEnumerable<Badge>> GetByCategoryAsync(string category)
    {
        return await _context.Badges
            .Where(b => b.Category == category && b.IsActive)
            .OrderBy(b => b.SortOrder)
            .ThenBy(b => b.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Kullanıcının kazandığı rozetleri getirir
    /// </summary>
    public async Task<IEnumerable<Badge>> GetUserBadgesAsync(Guid userId)
    {
        return await _context.Badges
            .Where(b => b.UserBadges.Any(ub => ub.UserId == userId))
            .OrderByDescending(b => b.UserBadges.First(ub => ub.UserId == userId).EarnedAt)
            .ToListAsync();
    }
}
