using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

/// <summary>
/// Admin kullanıcıları için repository implementasyonu
/// </summary>
public class AdminUserRepository : BaseRepository<AdminUser>, IAdminUserRepository
{
    public AdminUserRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Username'e göre admin kullanıcısını getirir
    /// </summary>
    public async Task<AdminUser?> GetByUsernameAsync(string username)
    {
        return await _context.AdminUsers
            .FirstOrDefaultAsync(a => a.Username == username);
    }

    /// <summary>
    /// Email'e göre admin kullanıcısını getirir
    /// </summary>
    public async Task<AdminUser?> GetByEmailAsync(string email)
    {
        return await _context.AdminUsers
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    /// <summary>
    /// Role göre admin kullanıcılarını getirir
    /// </summary>
    public async Task<List<AdminUser>> GetByRoleAsync(string role)
    {
        return await _context.AdminUsers
            .Where(a => a.Role.ToString() == role && a.IsActive)
            .OrderBy(a => a.Username)
            .ToListAsync();
    }

    /// <summary>
    /// Username'in müsait olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        return !await _context.AdminUsers
            .AnyAsync(a => a.Username == username);
    }

    /// <summary>
    /// Email'in müsait olup olmadığını kontrol eder
    /// </summary>
    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return !await _context.AdminUsers
            .AnyAsync(a => a.Email == email);
    }

    /// <summary>
    /// Aktif admin kullanıcılarını getirir
    /// </summary>
    public async Task<IEnumerable<AdminUser>> GetActiveAdminsAsync()
    {
        return await _context.AdminUsers
            .Where(a => a.IsActive)
            .OrderBy(a => a.Username)
            .ToListAsync();
    }
}
