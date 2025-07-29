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
    /// Aktif admin kullanıcılarını getirir
    /// </summary>
    public async Task<IEnumerable<AdminUser>> GetActiveAdminsAsync()
    {
        return await _context.AdminUsers
            .Where(a => a.IsActive)
            .OrderBy(a => a.FullName)
            .ToListAsync();
    }

    /// <summary>
    /// Role göre admin kullanıcılarını getirir
    /// </summary>
    public async Task<IEnumerable<AdminUser>> GetByRoleAsync(string role)
    {
        return await _context.AdminUsers
            .Where(a => a.Role == role && a.IsActive)
            .OrderBy(a => a.FullName)
            .ToListAsync();
    }
}
