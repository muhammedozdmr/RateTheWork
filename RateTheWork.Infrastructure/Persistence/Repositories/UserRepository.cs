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

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<User?> GetByTcIdentityNumberAsync(string tcIdentityNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.TCIdentityNumber == tcIdentityNumber);
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