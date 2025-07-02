using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces;

public interface IAdminUserRepository : IBaseRepository<AdminUser>
{
    Task<AdminUser?> GetByUsernameAsync(string username);
    Task<AdminUser?> GetByEmailAsync(string email);
    Task<List<AdminUser>> GetByRoleAsync(string role);
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);
}
