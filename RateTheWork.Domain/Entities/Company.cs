using System.Text.RegularExpressions;
using RateTheWork.Domain.Common;
using RateTheWork.Domain.Enums.Company;
using RateTheWork.Domain.Events.Company;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Common;
using RateTheWork.Domain.ValueObjects.Company;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şirket entity'si - Platformdaki şirket bilgilerini temsil eder.
/// ApprovableBaseEntity'den türer ve admin onayı gerektirir.
/// </summary>
public class Company : ApprovableBaseEntity, IAggregateRoot
{
    // Constants
    private const int MaxNameLength = 200;
    private const int MaxAddressLength = 500;
    private const int TaxIdLength = 10;
    private const int MersisNoLength = 16;
    private const int MaxDescriptionLength = 2000;

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Company(CompanyReviewStatistics reviewStatistics) : base()
    {
        ReviewStatistics = reviewStatistics;
    }

    private Company() : base()
    {
        throw new NotImplementedException();
    }

    // ========== TEMEL ŞİRKET BİLGİLERİ ==========

    public string Name { get; private set; } = string.Empty;
    public string TaxId { get; private set; } = string.Empty;
    public string MersisNo { get; private set; } = string.Empty;
    public CompanyType CompanyType { get; private set; }
    public int EstablishedYear { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? DeactivatedAt { get; private set; }
    public string? DeactivationReason { get; private set; } = string.Empty;

    // ========== SEKTÖR VE KATEGORİ ==========

    public CompanySector Sector { get; private set; }
    public string? SubSector { get; private set; }
    public string CompanySize { get; private set; } = "Small"; // Small, Medium, Large, Enterprise
    public List<string> Tags { get; private set; } = new();
    public string? Category { get; private set; }

    // ========== ÇALIŞAN BİLGİLERİ ==========

    public int? EmployeeCount { get; private set; }
    public string? EmployeeCountRange { get; private set; } // "1-10", "11-50", "51-200", vb.
    public DateTime? EmployeeCountUpdatedAt { get; private set; }
    public decimal? AverageEmployeeTenure { get; private set; } // Ortalama çalışma süresi (yıl)

    // ========== FİNANSAL BİLGİLER ==========

    public string? RevenueRange { get; private set; } // "0-1M", "1M-10M", "10M-100M", vb.
    public string? FundingStage { get; private set; } // Seed, Series A, IPO, vb.
    public decimal? MarketCap { get; private set; }

    // ========== İLETİŞİM BİLGİLERİ ==========

    public string Address { get; private set; } = string.Empty;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? District { get; private set; }
    public string? PostalCode { get; private set; }
    public string Country { get; private set; } = "Türkiye";

    public string PhoneNumber { get; private set; } = string.Empty;
    public string? FaxNumber { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string? HrEmail { get; private set; } // İK departmanı emaili
    public string? SupportEmail { get; private set; }

    public string WebsiteUrl { get; private set; } = string.Empty;
    public string? CareersPageUrl { get; private set; }

    // ========== SOSYAL MEDYA ==========

    public string? LinkedInUrl { get; set; }
    public string? XUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? YouTubeUrl { get; private set; }

    // ========== GÖRSELLER VE MEDYA ==========

    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; private set; }
    public List<string> GalleryImages { get; private set; } = new();
    public string? CompanyVideo { get; private set; }

    // ========== ÇALIŞMA BİLGİLERİ ==========

    public Dictionary<string, string>? WorkingHours { get; private set; } // Gün -> Saat mapping
    public bool? HasRemoteWork { get; private set; }
    public string? RemoteWorkPolicy { get; private set; }
    public List<string> Benefits { get; private set; } = new(); // Yan haklar

    // ========== İSTATİSTİKLER ==========

    public decimal AverageRating { get; private set; } = 0;
    public int TotalReviewCount { get; private set; } = 0;
    public int TotalReviews => TotalReviewCount; // Application katmanı için alias
    public DateTime? LastReviewDate { get; private set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public Dictionary<string, decimal> RatingBreakdown { get; private set; } = new(); // Kategori bazlı puanlar
    public Dictionary<string, int> ReviewCountByType { get; private set; } = new(); // Yorum tipi bazlı sayılar
    public CompanyReviewStatistics ReviewStatistics { get; set; }

    // ========== DOĞRULAMA BİLGİLERİ ==========

    public bool IsVerified { get; private set; } = false;
    public DateTime? VerifiedAt { get; private set; }
    public string? VerifiedBy { get; private set; }
    public string? VerificationMethod { get; private set; }
    public string? VerificationNotes { get; private set; }
    public Dictionary<string, object>? VerificationMetadata { get; private set; }
    public DateTime? UpdatedAt { get; set; }

    // ========== RİSK VE DEĞERLENDİRME ==========

    public decimal? RiskScore { get; private set; }
    public DateTime? RiskScoreUpdatedAt { get; private set; }
    public string? RiskLevel { get; private set; } // Low, Medium, High
    public List<string> ComplianceCertificates { get; private set; } = new(); // ISO, vb.

    // ========== BİRLEŞME VE SATIN ALMA ==========

    public bool IsMerged { get; private set; } = false;
    public string? MergedWithCompanyId { get; private set; }
    public DateTime? MergedAt { get; private set; }
    public string? ParentCompanyId { get; private set; } // Ana şirket
    public List<string> SubsidiaryIds { get; private set; } = new(); // Bağlı şirketler

    // ========== ŞUBE BİLGİLERİ ==========

    public int BranchCount { get; private set; } = 1;
    public List<CompanyBranch> Branches { get; private set; } = new();

    // ========== İLİŞKİLER ==========
    
    public virtual ICollection<Department> Departments { get; private set; } = new List<Department>();
    public virtual ICollection<CVApplication> CVApplications { get; private set; } = new List<CVApplication>();
    public virtual ICollection<ContractorReview> ContractorReviews { get; private set; } = new List<ContractorReview>();

    public new ApprovalStatus ApprovalStatus { get; private set; }

    /// <summary>
    /// Yeni şirket oluşturur (Factory method)
    /// </summary>
    public static Company Create
    (
        string name
        , string taxId
        , string mersisNo
        , CompanyType companyType
        , CompanySector sector
        , string city
        , string address
        , string phoneNumber
        , string email
        , string websiteUrl
        , int establishedYear
    )
    {
        // Validasyonlar
        ValidateName(name);
        ValidateTaxId(taxId);
        ValidateMersisNo(mersisNo);
        ValidateCity(city);
        ValidateAddress(address);
        ValidateEmail(email);
        ValidateWebsiteUrl(websiteUrl);
        ValidateEstablishedYear(establishedYear);

        var company = new Company
        {
            Name = name, TaxId = taxId, MersisNo = mersisNo, CompanyType = companyType, Sector = sector, City = city
            , Address = address, PhoneNumber = phoneNumber, Email = email.ToLowerInvariant(), WebsiteUrl = websiteUrl
            , EstablishedYear = establishedYear, Country = "Türkiye", AverageRating = 0, TotalReviewCount = 0
            , BranchCount = 1, IsActive = true
        };

        // Domain Event
        company.AddDomainEvent(new CompanyCreatedEvent(
            company.Id,
            company.Name,
            company.TaxId,
            company.MersisNo,
            company.CompanyType.ToString(),
            company.Sector.ToString(),
            company.City,
            company.Address,
            company.PhoneNumber,
            company.Email,
            company.WebsiteUrl,
            company.EstablishedYear,
            null, // CreatedByUserId
            "SYSTEM", // CreatedByIp
            company.CreatedAt
        ));

        return company;
    }

    /// <summary>
    /// Şirket açıklaması ekler/günceller
    /// </summary>
    public void SetDescription(string description)
    {
        if (description?.Length > MaxDescriptionLength)
            throw new BusinessRuleException($"Açıklama {MaxDescriptionLength} karakterden uzun olamaz.");

        Description = description;
        SetModifiedDate();
    }

    /// <summary>
    /// Çalışan bilgilerini günceller
    /// </summary>
    public void UpdateEmployeeInfo(int? employeeCount, string? employeeCountRange)
    {
        if (employeeCount.HasValue && employeeCount.Value < 0)
            throw new BusinessRuleException("Çalışan sayısı negatif olamaz.");

        EmployeeCount = employeeCount;
        EmployeeCountRange = employeeCountRange;
        EmployeeCountUpdatedAt = DateTime.UtcNow;

        // Şirket büyüklüğünü otomatik güncelle
        UpdateCompanySize();
        SetModifiedDate();
    }

    /// <summary>
    /// Finansal bilgileri günceller
    /// </summary>
    public void UpdateFinancialInfo(string? revenueRange, string? fundingStage, decimal? marketCap)
    {
        RevenueRange = revenueRange;
        FundingStage = fundingStage;
        MarketCap = marketCap;
        SetModifiedDate();
    }

    /// <summary>
    /// Detaylı adres bilgilerini günceller
    /// </summary>
    public void UpdateDetailedAddress
    (
        string address
        , string? addressLine2
        , string city
        , string? district
        , string? postalCode
    )
    {
        ValidateAddress(address);
        ValidateCity(city);

        Address = address;
        AddressLine2 = addressLine2;
        City = city;
        District = district;
        PostalCode = postalCode;
        SetModifiedDate();
    }

    /// <summary>
    /// İletişim bilgilerini günceller
    /// </summary>
    public void UpdateContactInfo
    (
        string phoneNumber
        , string email
        , string? hrEmail = null
        , string? supportEmail = null
        , string? faxNumber = null
    )
    {
        ValidateEmail(email);
        if (hrEmail != null) ValidateEmail(hrEmail);
        if (supportEmail != null) ValidateEmail(supportEmail);

        PhoneNumber = phoneNumber;
        Email = email.ToLowerInvariant();
        HrEmail = hrEmail?.ToLowerInvariant();
        SupportEmail = supportEmail?.ToLowerInvariant();
        FaxNumber = faxNumber;
        SetModifiedDate();
    }

    /// <summary>
    /// Sosyal medya linklerini günceller
    /// </summary>
    public void UpdateSocialMediaLinks
    (
        string? linkedInUrl = null
        , string? xUrl = null
        , string? instagramUrl = null
        , string? facebookUrl = null
        , string? youTubeUrl = null
    )
    {
        LinkedInUrl = linkedInUrl;
        XUrl = xUrl;
        InstagramUrl = instagramUrl;
        FacebookUrl = facebookUrl;
        YouTubeUrl = youTubeUrl;
        SetModifiedDate();
    }

    /// <summary>
    /// Çalışma bilgilerini günceller
    /// </summary>
    public void UpdateWorkInfo
    (
        Dictionary<string, string>? workingHours
        , bool? hasRemoteWork
        , string? remoteWorkPolicy
        , List<string>? benefits
    )
    {
        WorkingHours = workingHours;
        HasRemoteWork = hasRemoteWork;
        RemoteWorkPolicy = remoteWorkPolicy;
        if (benefits != null) Benefits = benefits;
        SetModifiedDate();
    }

    /// <summary>
    /// Görsel içerikleri günceller
    /// </summary>
    public void UpdateVisualContent
    (
        string? logoUrl
        , string? coverImageUrl
        , List<string>? galleryImages
        , string? companyVideo
    )
    {
        LogoUrl = logoUrl;
        CoverImageUrl = coverImageUrl;
        if (galleryImages != null) GalleryImages = galleryImages;
        CompanyVideo = companyVideo;
        SetModifiedDate();
    }

    /// <summary>
    /// Yorum istatistiklerini günceller
    /// </summary>
    public void UpdateReviewStatistics
    (
        decimal averageRating
        , int totalReviewCount
        , Dictionary<string, decimal>? ratingBreakdown = null
        , Dictionary<string, int>? reviewCountByType = null
    )
    {
        var oldRating = AverageRating;

        AverageRating = Math.Round(averageRating, 2);
        TotalReviewCount = totalReviewCount;
        LastReviewDate = DateTime.UtcNow;

        if (ratingBreakdown != null)
            RatingBreakdown = ratingBreakdown;

        if (reviewCountByType != null)
            ReviewCountByType = reviewCountByType;

        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new CompanyRatingUpdatedEvent(
            Id,
            oldRating,
            AverageRating,
            TotalReviewCount,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Şirket ID'sini günceller (merge işlemleri için)
    /// </summary>
    public void UpdateCompanyId(string newCompanyId)
    {
        if (string.IsNullOrWhiteSpace(newCompanyId))
            throw new ArgumentNullException(nameof(newCompanyId));

        if (newCompanyId == Id)
            throw new BusinessRuleException("Yeni ID mevcut ID ile aynı olamaz.");

        var oldId = Id;
        // ID güncelleme BaseEntity'de protected olduğu için reflection veya başka yöntem gerekebilir
        // Alternatif: Merge işlemi için özel bir metod kullanılabilir

        AddDomainEvent(new CompanyIdUpdatedEvent(oldId, newCompanyId, DateTime.UtcNow));
    }

    /// <summary>
    /// Şirketi doğrular
    /// </summary>
    public void Verify
    (
        string verifiedBy
        , string verificationMethod
        , string? verificationNotes = null
        , Dictionary<string, object>? metadata = null
    )
    {
        if (IsVerified)
            throw new InvalidOperationException("Şirket zaten doğrulanmış.");

        if (string.IsNullOrWhiteSpace(verifiedBy))
            throw new ArgumentNullException(nameof(verifiedBy));

        if (string.IsNullOrWhiteSpace(verificationMethod))
            throw new ArgumentNullException(nameof(verificationMethod));

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        VerifiedBy = verifiedBy;
        VerificationMethod = verificationMethod;
        VerificationNotes = verificationNotes;
        VerificationMetadata = metadata;

        // Eğer henüz onaylanmamışsa otomatik onayla
        if (!IsApproved)
        {
            IsApproved = true;
            ApprovedAt = DateTime.UtcNow;
            ApprovedBy = verifiedBy;

            ApprovalStatus = ApprovalStatus.Approved;
        }


        SetModifiedDate();

        // Domain event
        AddDomainEvent(new CompanyVerifiedEvent(
            Id,
            verificationMethod,
            verifiedBy,
            DateTime.UtcNow,
            metadata
        ));
    }

    /// <summary>
    /// Risk skorunu günceller
    /// </summary>
    public void UpdateRiskScore(decimal riskScore, string riskLevel)
    {
        if (riskScore < 0 || riskScore > 100)
            throw new ArgumentException("Risk skoru 0-100 arasında olmalıdır.");

        var validRiskLevels = new[] { "Low", "Medium", "High" };
        if (!validRiskLevels.Contains(riskLevel))
            throw new ArgumentException("Geçersiz risk seviyesi.");

        RiskScore = riskScore;
        RiskLevel = riskLevel;
        RiskScoreUpdatedAt = DateTime.UtcNow;
        SetModifiedDate();
    }

    /// <summary>
    /// Şube ekler
    /// </summary>
    public void AddBranch(CompanyBranch branch)
    {
        if (branch == null)
            throw new ArgumentNullException(nameof(branch));

        Branches.Add(branch);
        BranchCount = Branches.Count + 1; // +1 merkez ofis için
        SetModifiedDate();

        AddDomainEvent(new CompanyBranchAddedEvent(Id, branch.Id, branch.Name, branch.City, branch.IsHeadquarters
            , DateTime.UtcNow));
    }

    /// <summary>
    /// Şirket birleşmesi
    /// </summary>
    public void MergeWith(string targetCompanyId)
    {
        if (IsMerged)
            throw new InvalidOperationException("Şirket zaten birleşmiş.");

        IsMerged = true;
        MergedWithCompanyId = targetCompanyId;
        MergedAt = DateTime.UtcNow;
        IsActive = false;
        SetModifiedDate();

        AddDomainEvent(new CompanyMergedEvent(Id, targetCompanyId, DateTime.UtcNow));
    }

    /// <summary>
    /// Ana şirket ilişkisi kurar
    /// </summary>
    public void SetParentCompany(string parentCompanyId)
    {
        ParentCompanyId = parentCompanyId;
        SetModifiedDate();
    }

    /// <summary>
    /// Bağlı şirket ekler
    /// </summary>
    public void AddSubsidiary(string subsidiaryId)
    {
        if (!SubsidiaryIds.Contains(subsidiaryId))
        {
            SubsidiaryIds.Add(subsidiaryId);
            SetModifiedDate();
        }
    }

    // Private helper methods

    private void UpdateCompanySize()
    {
        CompanySize = EmployeeCount switch
        {
            null => CompanySize, <= 10 => "Micro", <= 50 => "Small", <= 250 => "Medium", <= 1000 => "Large"
            , _ => "Enterprise"
        };
    }

    /// <summary>
    /// Şirketi pasifleştirir
    /// </summary>
    public void Deactivate(string reason)
    {
        if (!IsActive)
            throw new BusinessRuleException("Şirket zaten pasif durumda.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException(nameof(reason), "Pasifleştirme nedeni belirtilmelidir.");

        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
        DeactivationReason = reason;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new CompanyDeactivatedEvent(Id, "SYSTEM", reason, DateTime.UtcNow));
    }

    /// <summary>
    /// Şirketi tekrar aktifleştirir
    /// </summary>
    public void Reactivate(string reactivatedBy)
    {
        if (IsActive)
            throw new BusinessRuleException("Şirket zaten aktif durumda.");

        IsActive = true;
        DeactivatedAt = null;
        DeactivationReason = string.Empty;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new CompanyReactivatedEvent(Id, "SYSTEM", reactivatedBy, DateTime.UtcNow));
    }

    /// <summary>
    /// Metadata ekler veya günceller
    /// </summary>
    public void AddMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
        SetModifiedDate();
    }

    /// <summary>
    /// Metadata siler
    /// </summary>
    public void RemoveMetadata(string key)
    {
        if (Metadata?.ContainsKey(key) == true)
        {
            Metadata.Remove(key);
            SetModifiedDate();
        }
    }

    // Validation methods

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (name.Length > MaxNameLength)
            throw new BusinessRuleException($"Şirket adı {MaxNameLength} karakterden uzun olamaz.");
    }

    private static void ValidateTaxId(string taxId)
    {
        if (string.IsNullOrWhiteSpace(taxId))
            throw new ArgumentNullException(nameof(taxId));

        if (taxId.Length != TaxIdLength || !taxId.All(char.IsDigit))
            throw new BusinessRuleException(
                $"Vergi numarası {TaxIdLength} haneli olmalı ve sadece rakamlardan oluşmalıdır.");
    }

    private static void ValidateMersisNo(string mersisNo)
    {
        if (string.IsNullOrWhiteSpace(mersisNo))
            throw new ArgumentNullException(nameof(mersisNo));

        if (mersisNo.Length != MersisNoLength)
            throw new BusinessRuleException($"MERSİS numarası {MersisNoLength} haneli olmalıdır.");
    }


    private static void ValidateCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentNullException(nameof(city));
    }

    private static void ValidateAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentNullException(nameof(address));

        if (address.Length > MaxAddressLength)
            throw new BusinessRuleException($"Adres {MaxAddressLength} karakterden uzun olamaz.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new BusinessRuleException("Geçersiz email formatı.");
    }

    private static void ValidateWebsiteUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new BusinessRuleException("Geçersiz website URL'i.");
        }
    }

    private static void ValidateEstablishedYear(int year)
    {
        var currentYear = DateTime.Now.Year;
        if (year < 1900 || year > currentYear)
            throw new BusinessRuleException($"Kuruluş yılı 1900 ile {currentYear} arasında olmalıdır.");
    }
    
    /// <summary>
    /// Şirketin temel bilgilerini günceller
    /// </summary>
    public void UpdateBasicInfo(
        string? name = null,
        CompanySector? sector = null,
        string? websiteUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(name) && name != Name)
        {
            ValidateName(name);
            Name = name;
        }
        
        if (sector.HasValue && sector.Value != Sector)
        {
            Sector = sector.Value;
        }
        
        if (!string.IsNullOrWhiteSpace(websiteUrl) && websiteUrl != WebsiteUrl)
        {
            ValidateWebsiteUrl(websiteUrl);
            WebsiteUrl = websiteUrl;
        }
        
        SetModifiedDate();
        
        // Domain Event
        var updatedFields = new List<string>();
        var oldValues = new Dictionary<string, object>();
        var newValues = new Dictionary<string, object>();
        
        if (!string.IsNullOrWhiteSpace(name) && name != Name)
        {
            updatedFields.Add("Name");
            oldValues["Name"] = Name;
            newValues["Name"] = name;
        }
        
        if (sector.HasValue && sector.Value != Sector)
        {
            updatedFields.Add("Sector");
            oldValues["Sector"] = Sector.ToString();
            newValues["Sector"] = sector.Value.ToString();
        }
        
        if (!string.IsNullOrWhiteSpace(websiteUrl) && websiteUrl != WebsiteUrl)
        {
            updatedFields.Add("WebsiteUrl");
            oldValues["WebsiteUrl"] = WebsiteUrl;
            newValues["WebsiteUrl"] = websiteUrl;
        }
        
        if (updatedFields.Any())
        {
            AddDomainEvent(new CompanyInfoUpdatedEvent(
                companyId: Id,
                updatedFields: updatedFields.ToArray(),
                oldValues: oldValues,
                newValues: newValues,
                updatedBy: ModifiedBy ?? "System",
                updateReason: null,
                updatedAt: DateTime.UtcNow
            ));
        }
    }
}
