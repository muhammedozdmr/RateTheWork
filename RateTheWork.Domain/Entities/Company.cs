using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events.Company;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Common;

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

    // ========== TEMEL ŞİRKET BİLGİLERİ ==========
    
    public string Name { get; private set; } = string.Empty;
    public string TaxId { get; private set; } = string.Empty;
    public string MersisNo { get; private set; } = string.Empty;
    public string CompanyType { get; private set; } = string.Empty; // Ltd. Şti., A.Ş., vb.
    public int EstablishedYear { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    // ========== SEKTÖR VE KATEGORİ ==========
    
    public string Sector { get; private set; } = string.Empty;
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
    
    public string? LinkedInUrl { get; private set; }
    public string? XUrl { get; private set; }
    public string? InstagramUrl { get; private set; }
    public string? FacebookUrl { get; private set; }
    public string? YouTubeUrl { get; private set; }
    
    // ========== GÖRSELLER VE MEDYA ==========
    
    public string? LogoUrl { get; private set; }
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
    public DateTime? LastReviewDate { get; private set; }
    public Dictionary<string, decimal> RatingBreakdown { get; private set; } = new(); // Kategori bazlı puanlar
    public Dictionary<string, int> ReviewCountByType { get; private set; } = new(); // Yorum tipi bazlı sayılar
    
    // ========== DOĞRULAMA BİLGİLERİ ==========
    
    public bool IsVerified { get; private set; } = false;
    public DateTime? VerifiedAt { get; private set; }
    public string? VerifiedBy { get; private set; }
    public string? VerificationMethod { get; private set; }
    public string? VerificationNotes { get; private set; }
    public Dictionary<string, object>? VerificationMetadata { get; private set; }
    
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

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Company() : base()
    {
    }

    /// <summary>
    /// Yeni şirket oluşturur (Factory method)
    /// </summary>
    public static Company Create(
        string name,
        string taxId,
        string mersisNo,
        string companyType,
        string sector,
        string city,
        string address,
        string phoneNumber,
        string email,
        string websiteUrl,
        int establishedYear)
    {
        // Validasyonlar
        ValidateName(name);
        ValidateTaxId(taxId);
        ValidateMersisNo(mersisNo);
        ValidateCompanyType(companyType);
        ValidateSector(sector);
        ValidateCity(city);
        ValidateAddress(address);
        ValidateEmail(email);
        ValidateWebsiteUrl(websiteUrl);
        ValidateEstablishedYear(establishedYear);

        var company = new Company
        {
            Name = name,
            TaxId = taxId,
            MersisNo = mersisNo,
            CompanyType = companyType,
            Sector = sector,
            City = city,
            Address = address,
            PhoneNumber = phoneNumber,
            Email = email.ToLowerInvariant(),
            WebsiteUrl = websiteUrl,
            EstablishedYear = establishedYear,
            Country = "Türkiye",
            AverageRating = 0,
            TotalReviewCount = 0,
            BranchCount = 1,
            IsActive = true
        };

        // Domain Event
        company.AddDomainEvent(new CompanyCreatedEvent(
            company.Id,
            company.Name,
            company.TaxId,
            company.MersisNo,
            company.Sector,
            DateTime.UtcNow
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
    public void UpdateDetailedAddress(
        string address,
        string? addressLine2,
        string city,
        string? district,
        string? postalCode)
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
    public void UpdateContactInfo(
        string phoneNumber,
        string email,
        string? hrEmail = null,
        string? supportEmail = null,
        string? faxNumber = null)
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
    public void UpdateSocialMediaLinks(
        string? linkedInUrl = null,
        string? xUrl = null,
        string? instagramUrl = null,
        string? facebookUrl = null,
        string? youTubeUrl = null)
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
    public void UpdateWorkInfo(
        Dictionary<string, string>? workingHours,
        bool? hasRemoteWork,
        string? remoteWorkPolicy,
        List<string>? benefits)
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
    public void UpdateVisualContent(
        string? logoUrl,
        string? coverImageUrl,
        List<string>? galleryImages,
        string? companyVideo)
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
    public void UpdateReviewStatistics(
        decimal averageRating,
        int totalReviewCount,
        Dictionary<string, decimal>? ratingBreakdown = null,
        Dictionary<string, int>? reviewCountByType = null)
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
    /// Şirketi doğrular
    /// </summary>
    public void Verify(
        string verifiedBy,
        string verificationMethod,
        string? verificationNotes = null,
        Dictionary<string, object>? metadata = null)
    {
        if (IsVerified)
            throw new InvalidOperationException("Şirket zaten doğrulanmış.");
            
        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        VerifiedBy = verifiedBy;
        VerificationMethod = verificationMethod;
        VerificationNotes = verificationNotes;
        VerificationMetadata = metadata;
        SetModifiedDate();
        
        // Domain event
        AddDomainEvent(new CompanyVerifiedEvent(Id, verificationMethod, verifiedBy,DateTime.UtcNow));
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
        
        AddDomainEvent(new CompanyBranchAddedEvent(Id, branch.Id, branch.Name, branch.City, branch.IsHeadquarters, branch.CreatedAt));
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
            null => CompanySize,
            <= 10 => "Micro",
            <= 50 => "Small",
            <= 250 => "Medium",
            <= 1000 => "Large",
            _ => "Enterprise"
        };
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
            throw new BusinessRuleException($"Vergi numarası {TaxIdLength} haneli olmalı ve sadece rakamlardan oluşmalıdır.");
    }

    private static void ValidateMersisNo(string mersisNo)
    {
        if (string.IsNullOrWhiteSpace(mersisNo))
            throw new ArgumentNullException(nameof(mersisNo));

        if (mersisNo.Length != MersisNoLength)
            throw new BusinessRuleException($"MERSİS numarası {MersisNoLength} haneli olmalıdır.");
    }

    private static void ValidateCompanyType(string companyType)
    {
        if (string.IsNullOrWhiteSpace(companyType))
            throw new ArgumentNullException(nameof(companyType));

        var validTypes = new[] { "A.Ş.", "Ltd. Şti.", "Koll. Şti.", "Kom. Şti.", "Adi Ort.", "Şahıs", "Koop.", "Dernek", "Vakıf", "Kamu" };
        
        if (!validTypes.Contains(companyType))
            throw new BusinessRuleException("Geçersiz şirket türü.");
    }

    private static void ValidateSector(string sector)
    {
        if (string.IsNullOrWhiteSpace(sector))
            throw new ArgumentNullException(nameof(sector));

        var validSectors = new[] { 
            "Teknoloji", "Finans", "Sağlık", "Eğitim", "Perakende", 
            "Üretim", "İnşaat", "Turizm", "Medya", "Telekomünikasyon", 
            "Enerji", "Otomotiv", "Yiyecek & İçecek", "Lojistik", 
            "Gayrimenkul", "Danışmanlık", "Hukuk", "Tarım", "Tekstil", "Diğer" 
        };

        if (!validSectors.Contains(sector))
            throw new BusinessRuleException("Geçersiz sektör.");
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

        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
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
}
