using Microsoft.EntityFrameworkCore;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

/// <summary>
/// Departman yönetimi için repository implementasyonu
/// </summary>
public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Şirkete ait departmanları getirir
    /// </summary>
    public async Task<List<Department>> GetByCompanyIdAsync(string companyId)
    {
        return await _context.Departments
            .Where(d => d.CompanyId == companyId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Şirkete ait aktif departmanları getirir
    /// </summary>
    public async Task<List<Department>> GetActiveByCompanyIdAsync(string companyId)
    {
        return await _context.Departments
            .Where(d => d.CompanyId == companyId && d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    /// <summary>
    /// İsme göre departman arar
    /// </summary>
    public async Task<Department?> GetByNameAsync(string companyId, string name)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.CompanyId == companyId && d.Name == name);
    }

    /// <summary>
    /// Manager'a göre departmanları getirir
    /// </summary>
    public async Task<List<Department>> GetByManagerIdAsync(string managerId)
    {
        return await _context.Departments
            .Where(d => d.ManagerId == managerId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Aktif departmanları getirir
    /// </summary>
    public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
    {
        return await _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Şube bazında departmanları getirir
    /// </summary>
    public async Task<IEnumerable<Department>> GetByBranchIdAsync(Guid branchId)
    {
        return await _context.Departments
            .Where(d => d.BranchId == branchId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Departman koduna göre departman getirir
    /// </summary>
    public async Task<Department?> GetByCodeAsync(string code)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Code == code);
    }

    /// <summary>
    /// Üst departmana göre alt departmanları getirir
    /// </summary>
    public async Task<IEnumerable<Department>> GetSubDepartmentsAsync(Guid parentDepartmentId)
    {
        return await _context.Departments
            .Where(d => d.ParentDepartmentId == parentDepartmentId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Hiyerarşik departman yapısını getirir
    /// </summary>
    public async Task<IEnumerable<Department>> GetHierarchyAsync(Guid? rootDepartmentId = null)
    {
        return await _context.Departments
            .Where(d => rootDepartmentId == null
                ? d.ParentDepartmentId == null
                : d.ParentDepartmentId == rootDepartmentId)
            .Include(d => d.SubDepartments)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }
}
