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
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByAnonymousUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.AnonymousUsername == username);
    }

    public async Task<User?> GetByTcIdentityAsync(string tcIdentity)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.EncryptedTcIdentityNumber == tcIdentity);
    }

    public async Task<List<Review>> GetUserReviewsAsync(string userId, int page = 1, int pageSize = 10)
    {
        // TODO: Implement when Review relationship is properly configured
        return await Task.FromResult(new List<Review>());
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

        // TODO: Implement when Review entity is properly configured
        return await Task.FromResult(false);
    }

    public async Task<bool> IsUserBannedAsync(string userId)
    {
        // TODO: Implement when Ban relationship is properly configured
        return await Task.FromResult(false);
    }

    public async Task<int> GetUserWarningCountAsync(string userId)
    {
        // TODO: Implement when Warning relationship is properly configured
        return await Task.FromResult(0);
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> IsUsernameTakenAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.AnonymousUsername == username);
    }

    public void Update(User user)
    {
        _dbSet.Update(user);
    }

    // Override to match IUserRepository interface
    public new async Task UpdateAsync(User user)
    {
        await base.UpdateAsync(user);
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
            query = query.Where(u => u.Id != excludeUserId.Value.ToString());
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null)
    {
        var query = _dbSet.Where(u => u.AnonymousUsername == username);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value.ToString());
        }

        return !await query.AnyAsync();
    }

    public async Task<User?> GetWithDetailsAsync(Guid userId)
    {
        // TODO: Include related entities when properly configured
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Id == userId.ToString());
    }
}