using RateTheWork.Domain.Common;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şirket departmanı entity'si
/// </summary>
public class Department : AuditableBaseEntity
{
    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Department() : base()
    {
    }

    // Properties
    public string CompanyId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public int OpenPositionCount { get; private set; } = 0;
    public int EmployeeCount { get; private set; } = 0;
    public string? ManagerId { get; private set; }
    public string? ContactEmail { get; private set; }
    public Guid? BranchId { get; private set; }
    public Guid? ParentDepartmentId { get; private set; }

    // Navigation
    public virtual Company? Company { get; private set; }
    public virtual Department? ParentDepartment { get; private set; }
    public virtual ICollection<Department> SubDepartments { get; private set; } = new List<Department>();
    public virtual ICollection<CVApplication> CVApplications { get; private set; } = new List<CVApplication>();

    /// <summary>
    /// Yeni departman oluşturur
    /// </summary>
    public static Department Create
    (
        string companyId
        , string name
        , string? description = null
        , string? managerId = null
        , string? contactEmail = null
    )
    {
        if (string.IsNullOrWhiteSpace(companyId))
            throw new ArgumentNullException(nameof(companyId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (name.Length > 100)
            throw new BusinessRuleException("Departman adı 100 karakterden uzun olamaz.");

        var department = new Department
        {
            CompanyId = companyId, Name = name.Trim(), Description = description?.Trim(), ManagerId = managerId
            , ContactEmail = contactEmail?.ToLowerInvariant(), IsActive = true
        };

        return department;
    }

    /// <summary>
    /// Departman bilgilerini günceller
    /// </summary>
    public void Update
    (
        string? name = null
        , string? description = null
        , string? managerId = null
        , string? contactEmail = null
    )
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            if (name.Length > 100)
                throw new BusinessRuleException("Departman adı 100 karakterden uzun olamaz.");
            Name = name.Trim();
        }

        if (description != null)
            Description = description.Trim();

        if (managerId != null)
            ManagerId = managerId;

        if (contactEmail != null)
            ContactEmail = contactEmail.ToLowerInvariant();

        SetModifiedDate();
    }

    /// <summary>
    /// Departmanı aktif/pasif yapar
    /// </summary>
    public void SetActiveStatus(bool isActive)
    {
        IsActive = isActive;
        SetModifiedDate();
    }

    /// <summary>
    /// Açık pozisyon sayısını günceller
    /// </summary>
    public void UpdateOpenPositionCount(int count)
    {
        if (count < 0)
            throw new BusinessRuleException("Açık pozisyon sayısı negatif olamaz.");

        OpenPositionCount = count;
        SetModifiedDate();
    }

    /// <summary>
    /// Çalışan sayısını günceller
    /// </summary>
    public void UpdateEmployeeCount(int count)
    {
        if (count < 0)
            throw new BusinessRuleException("Çalışan sayısı negatif olamaz.");

        EmployeeCount = count;
        SetModifiedDate();
    }
}
