using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByAnonymousUsernameAsync(string username)
    {
        return await _dbSet
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<User?> GetByTcIdentityAsync(string tcIdentity)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.TCIdentityNumber == tcIdentity);
    }

    public async Task<List<Review>> GetUserReviewsAsync(string userId, int page = 1, int pageSize = 10)
    {
        if (!Guid.TryParse(userId, out var guidId))
            return new List<Review>();

        var user = await _dbSet
            .Include(u => u.Reviews)
            .FirstOrDefaultAsync(u => u.Id == guidId);

        if (user == null)
            return new List<Review>();

        return user.Reviews
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<List<UserBadge>> GetUserBadgesAsync(string userId)
    {
        // UserBadge ilişkisi kurulmadığı için boş liste dönüyoruz
        // TODO: UserBadge ilişkisi eklendiğinde implement edilecek
        return await Task.FromResult(new List<UserBadge>());
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        // Notification ilişkisi kurulmadığı için boş liste dönüyoruz
        // TODO: Notification ilişkisi eklendiğinde implement edilecek
        return await Task.FromResult(new List<Notification>());
    }

    public async Task<bool> HasUserReviewedCompanyAsync(string? userId, string? companyId, string commentType)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(companyId))
            return false;

        if (!Guid.TryParse(userId, out var userGuid) || !Guid.TryParse(companyId, out var companyGuid))
            return false;

        return await _context.Reviews
            .AnyAsync(r => r.UserId == userGuid && 
                          r.CompanyId == companyGuid && 
                          r.CommentType.ToString() == commentType);
    }

    public async Task<bool> IsUserBannedAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var guidId))
            return false;

        var user = await _dbSet
            .Include(u => u.Bans)
            .FirstOrDefaultAsync(u => u.Id == guidId);

        return user?.Bans.Any(b => b.IsActive && b.EndDate > DateTime.UtcNow) ?? false;
    }

    public async Task<int> GetUserWarningCountAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var guidId))
            return 0;

        var user = await _dbSet
            .Include(u => u.Warnings)
            .FirstOrDefaultAsync(u => u.Id == guidId);

        return user?.Warnings.Count(w => w.IsActive) ?? 0;
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> IsUsernameTakenAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.UserName == username);
    }

    public void Update(User user)
    {
        _dbSet.Update(user);
    }

    // Eski interface metodları için uyumluluk
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await GetByAnonymousUsernameAsync(username);
    }

    public async Task<User?> GetByTcIdentityNumberAsync(string tcIdentityNumber)
    {
        return await GetByTcIdentityAsync(tcIdentityNumber);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null)
    {
        var query = _dbSet.Where(u => u.Email == email);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null)
    {
        var query = _dbSet.Where(u => u.UserName == username);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<User?> GetWithDetailsAsync(Guid userId)
    {
        return await _dbSet
            .Include(u => u.Reviews)
            .Include(u => u.Subscription)
            .Include(u => u.Warnings)
            .Include(u => u.Bans)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
}