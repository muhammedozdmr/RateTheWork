namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şirket şubesi entity'si
/// </summary>
public class CompanyBranch
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string City { get; private set; }
    public string? District { get; private set; }
    public string Address { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public int? EmployeeCount { get; private set; }
    public bool IsHeadquarters { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// EF Core için parametresiz constructor
    /// </summary>
    private CompanyBranch() 
    { 
        Id = string.Empty;
        Name = string.Empty;
        City = string.Empty;
        Address = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Yeni şube oluşturur
    /// </summary>
    public static CompanyBranch Create(
        string name,
        string city,
        string address,
        bool isHeadquarters = false,
        string? district = null,
        string? phoneNumber = null,
        string? email = null,
        int? employeeCount = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Şube adı boş olamaz.", nameof(name));
            
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Şehir bilgisi boş olamaz.", nameof(city));
            
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Adres bilgisi boş olamaz.", nameof(address));
            
        if (employeeCount.HasValue && employeeCount.Value < 0)
            throw new ArgumentException("Çalışan sayısı negatif olamaz.", nameof(employeeCount));

        return new CompanyBranch
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            City = city,
            District = district,
            Address = address,
            PhoneNumber = phoneNumber,
            Email = email?.ToLowerInvariant(),
            EmployeeCount = employeeCount,
            IsHeadquarters = isHeadquarters,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Şube bilgilerini günceller
    /// </summary>
    public void Update(
        string? name = null,
        string? city = null,
        string? district = null,
        string? address = null,
        string? phoneNumber = null,
        string? email = null,
        int? employeeCount = null)
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
        
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Şubeyi merkez ofis olarak işaretle
    /// </summary>
    public void SetAsHeadquarters()
    {
        IsHeadquarters = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Şubenin merkez ofis işaretini kaldır
    /// </summary>
    public void UnsetAsHeadquarters()
    {
        IsHeadquarters = false;
        UpdatedAt = DateTime.UtcNow;
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

