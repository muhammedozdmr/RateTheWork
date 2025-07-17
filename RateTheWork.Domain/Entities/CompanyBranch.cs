using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events.CompanyBranch;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şirket şubesi entity'si
/// </summary>
public class CompanyBranch : BaseEntity
{
    /// <summary>
    /// EF Core için parametresiz constructor
    /// </summary>
    private CompanyBranch() : base()
    {
        Name = string.Empty;
        City = string.Empty;
        Address = string.Empty;
    }

    public string CompanyId { get; private set; } = string.Empty;
    public string Name { get; private set; }
    public string City { get; private set; }
    public string? District { get; private set; }
    public string Address { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public int? EmployeeCount { get; private set; }
    public bool IsHeadquarters { get; private set; }
    public string? Country { get; private set; } = "Türkiye";
    public string? PostalCode { get; private set; }
    public string BranchType { get; private set; } = "Branch"; // Branch, Office, Store, Warehouse
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Yeni şube oluşturur
    /// </summary>
    public static CompanyBranch Create
    (
        string companyId
        , string name
        , string city
        , string address
        , bool isHeadquarters = false
        , string? district = null
        , string? phoneNumber = null
        , string? email = null
        , int? employeeCount = null
        , string? postalCode = null
        , string branchType = "Branch"
        , string? country = "Türkiye"
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Şube adı boş olamaz.", nameof(name));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Şehir bilgisi boş olamaz.", nameof(city));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Adres bilgisi boş olamaz.", nameof(address));

        if (employeeCount.HasValue && employeeCount.Value < 0)
            throw new ArgumentException("Çalışan sayısı negatif olamaz.", nameof(employeeCount));

        var branch = new CompanyBranch
        {
            CompanyId = companyId, Name = name, City = city, District = district, Address = address
            , PhoneNumber = phoneNumber, Email = email?.ToLowerInvariant(), EmployeeCount = employeeCount
            , IsHeadquarters = isHeadquarters, PostalCode = postalCode, BranchType = branchType, Country = country
            , IsActive = true
        };

        // Domain Event
        branch.AddDomainEvent(new CompanyBranchCreatedEvent(
            branch.Id,
            companyId,
            name,
            branchType,
            address,
            city,
            district,
            postalCode,
            country ?? "Türkiye",
            phoneNumber,
            email,
            isHeadquarters,
            "SYSTEM",
            DateTime.UtcNow
        ));

        return branch;
    }

    /// <summary>
    /// Genel merkez oluşturur
    /// </summary>
    public static CompanyBranch CreateHeadquarters
    (
        string companyId
        , string name
        , string city
        , string address
        , string? district = null
        , string? phoneNumber = null
        , string? email = null
        , int? employeeCount = null
        , string? postalCode = null
    )
    {
        return Create(
            companyId,
            name,
            city,
            address,
            true, // isHeadquarters
            district,
            phoneNumber,
            email,
            employeeCount,
            postalCode,
            "Headquarters"
        );
    }

    /// <summary>
    /// Mağaza oluşturur
    /// </summary>
    public static CompanyBranch CreateStore
    (
        string companyId
        , string name
        , string city
        , string address
        , string? district = null
        , string? phoneNumber = null
        , string? email = null
    )
    {
        return Create(
            companyId,
            name,
            city,
            address,
            false,
            district,
            phoneNumber,
            email,
            null,
            null,
            "Store"
        );
    }

    /// <summary>
    /// Depo oluşturur
    /// </summary>
    public static CompanyBranch CreateWarehouse
    (
        string companyId
        , string name
        , string city
        , string address
        , string? postalCode = null
        , int? capacity = null
    )
    {
        return Create(
            companyId,
            name,
            city,
            address,
            false,
            null,
            null,
            null,
            capacity,
            postalCode,
            "Warehouse"
        );
    }

    /// <summary>
    /// Şube bilgilerini günceller
    /// </summary>
    public void Update
    (
        string? name = null
        , string? city = null
        , string? district = null
        , string? address = null
        , string? phoneNumber = null
        , string? email = null
        , int? employeeCount = null
    )
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;

        if (!string.IsNullOrWhiteSpace(city))
            City = city;

        if (district != null) // null olabilir
            District = district;

        if (!string.IsNullOrWhiteSpace(address))
            Address = address;

        if (phoneNumber != null) // null olabilir
            PhoneNumber = phoneNumber;

        if (email != null) // null olabilir
            Email = email.ToLowerInvariant();

        if (employeeCount.HasValue)
        {
            if (employeeCount.Value < 0)
                throw new ArgumentException("Çalışan sayısı negatif olamaz.");
            EmployeeCount = employeeCount;
        }

        SetModifiedDate();

        // Domain Event
        var updatedFields = new List<string>();
        if (!string.IsNullOrWhiteSpace(name)) updatedFields.Add("Name");
        if (!string.IsNullOrWhiteSpace(city)) updatedFields.Add("City");
        if (district != null) updatedFields.Add("District");
        if (!string.IsNullOrWhiteSpace(address)) updatedFields.Add("Address");
        if (phoneNumber != null) updatedFields.Add("PhoneNumber");
        if (email != null) updatedFields.Add("Email");
        if (employeeCount.HasValue) updatedFields.Add("EmployeeCount");

        if (updatedFields.Any())
        {
            AddDomainEvent(new CompanyBranchUpdatedEvent(
                Id,
                CompanyId,
                updatedFields.ToArray(),
                null, // OldValues
                null, // NewValues
                "SYSTEM",
                DateTime.UtcNow
            ));
        }
    }

    /// <summary>
    /// Şubeyi merkez ofis olarak işaretle
    /// </summary>
    public void SetAsHeadquarters(string setBy)
    {
        if (IsHeadquarters)
            throw new BusinessRuleException("Şube zaten merkez ofis olarak işaretlenmiş.");

        IsHeadquarters = true;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new CompanyBranchSetAsHeadquartersEvent(
            Id,
            CompanyId,
            null, // PreviousHeadquartersBranchId
            setBy,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Şubenin merkez ofis işaretini kaldır
    /// </summary>
    public void UnsetAsHeadquarters()
    {
        if (!IsHeadquarters)
            throw new BusinessRuleException("Şube zaten merkez ofis değil.");

        IsHeadquarters = false;
        SetModifiedDate();
    }

    /// <summary>
    /// Şubeyi deaktif eder
    /// </summary>
    public void Deactivate(string deactivatedBy, string reason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Şube zaten deaktif.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason));

        IsActive = false;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new CompanyBranchDeactivatedEvent(
            Id,
            CompanyId,
            deactivatedBy,
            reason,
            null, // ReactivationDate
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Şubeyi yeniden aktif eder
    /// </summary>
    public void Reactivate(string reactivatedBy)
    {
        if (IsActive)
            throw new BusinessRuleException("Şube zaten aktif.");

        IsActive = true;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new CompanyBranchReactivatedEvent(
            Id,
            CompanyId,
            reactivatedBy,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// İki şubenin eşit olup olmadığını kontrol eder
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not CompanyBranch other)
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Hash code üretir
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// String representation
    /// </summary>
    public override string ToString()
    {
        return $"{Name} - {City}{(IsHeadquarters ? " (Merkez)" : "")}";
    }
}
