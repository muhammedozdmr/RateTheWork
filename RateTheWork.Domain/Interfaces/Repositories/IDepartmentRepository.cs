using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Departman repository interface
/// </summary>
public interface IDepartmentRepository : IBaseRepository<Department>
{
    /// <summary>
    /// Şirkete ait departmanları getirir
    /// </summary>
    Task<List<Department>> GetByCompanyIdAsync(string companyId);
    
    /// <summary>
    /// Şirkete ait aktif departmanları getirir
    /// </summary>
    Task<List<Department>> GetActiveByCompanyIdAsync(string companyId);
    
    /// <summary>
    /// İsme göre departman arar
    /// </summary>
    Task<Department?> GetByNameAsync(string companyId, string name);
    
    /// <summary>
    /// Manager'a göre departmanları getirir
    /// </summary>
    Task<List<Department>> GetByManagerIdAsync(string managerId);
}