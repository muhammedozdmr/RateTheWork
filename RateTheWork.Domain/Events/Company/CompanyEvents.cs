namespace RateTheWork.Domain.Events.Company;

/// <summary>
/// 1. Şirket oluşturuldu event'i
/// </summary>
public class CompanyCreatedEvent : DomainEventBase
{
    public CompanyCreatedEvent
    (
        string? companyId
        , string name
        , string taxId
        , string mersisNo
        , string companyType
        , string sector
        , string city
        , string? address
        , string? phoneNumber
        , string? email
        , string? websiteUrl
        , int? establishedYear
        , string? createdByUserId
        , string createdByIp
        , DateTime createdAt
    ) : base()
    {
        CompanyId = companyId;
        Name = name;
        TaxId = taxId;
        MersisNo = mersisNo;
        CompanyType = companyType;
        Sector = sector;
        City = city;
        Address = address;
        PhoneNumber = phoneNumber;
        Email = email;
        WebsiteUrl = websiteUrl;
        EstablishedYear = establishedYear;
        CreatedByUserId = createdByUserId;
        CreatedByIp = createdByIp;
        CreatedAt = createdAt;
    }

    public string? CompanyId { get; }
    public string Name { get; }
    public string TaxId { get; }
    public string MersisNo { get; }
    public string CompanyType { get; }
    public string Sector { get; }
    public string City { get; }
    public string? Address { get; }
    public string? PhoneNumber { get; }
    public string? Email { get; }
    public string? WebsiteUrl { get; }
    public int? EstablishedYear { get; }
    public string? CreatedByUserId { get; }
    public string CreatedByIp { get; }
    public DateTime CreatedAt { get; }
}

/// <summary>
/// 2. Şirket onaylandı event'i
/// </summary>
public class CompanyApprovedEvent : DomainEventBase
{
    public CompanyApprovedEvent
    (
        string? companyId
        , string approvedBy
        , string? notes
        , DateTime approvedAt
    ) : base()
    {
        CompanyId = companyId;
        ApprovedBy = approvedBy;
        Notes = notes;
        ApprovedAt = approvedAt;
    }

    public string? CompanyId { get; }
    public string ApprovedBy { get; }
    public string? Notes { get; }
    public DateTime ApprovedAt { get; }
}

/// <summary>
/// 3. Şirket reddedildi event'i
/// </summary>
public class CompanyRejectedEvent : DomainEventBase
{
    public CompanyRejectedEvent
    (
        string? companyId
        , string rejectedBy
        , string reason
        , DateTime rejectedAt
    ) : base()
    {
        CompanyId = companyId;
        RejectedBy = rejectedBy;
        Reason = reason;
        RejectedAt = rejectedAt;
    }

    public string? CompanyId { get; }
    public string RejectedBy { get; }
    public string Reason { get; }
    public DateTime RejectedAt { get; }
}

/// <summary>
/// 4. Şirket bilgileri güncellendi event'i
/// </summary>
public class CompanyInfoUpdatedEvent : DomainEventBase
{
    public CompanyInfoUpdatedEvent
    (
        string? companyId
        , string[] updatedFields
        , Dictionary<string, object>? oldValues
        , Dictionary<string, object>? newValues
        , string updatedBy
        , string? updateReason
        , DateTime updatedAt
    ) : base()
    {
        CompanyId = companyId;
        UpdatedFields = updatedFields;
        OldValues = oldValues;
        NewValues = newValues;
        UpdatedBy = updatedBy;
        UpdateReason = updateReason;
        UpdatedAt = updatedAt;
    }

    public string? CompanyId { get; }
    public string[] UpdatedFields { get; }
    public Dictionary<string, object>? OldValues { get; }
    public Dictionary<string, object>? NewValues { get; }
    public string UpdatedBy { get; }
    public string? UpdateReason { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 5. Şirket logosu güncellendi event'i
/// </summary>
public class CompanyLogoUpdatedEvent : DomainEventBase
{
    public CompanyLogoUpdatedEvent
    (
        string? companyId
        , string logoUrl
        , DateTime updatedAt
    ) : base()
    {
        CompanyId = companyId;
        LogoUrl = logoUrl;
        UpdatedAt = updatedAt;
    }

    public string? CompanyId { get; }
    public string LogoUrl { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 6. Şirket puanı güncellendi event'i
/// </summary>
public class CompanyRatingUpdatedEvent : DomainEventBase
{
    public CompanyRatingUpdatedEvent
    (
        string? companyId
        , decimal oldRating
        , decimal newRating
        , int totalReviews
        , DateTime updatedAt
    ) : base()
    {
        CompanyId = companyId;
        OldRating = oldRating;
        NewRating = newRating;
        TotalReviews = totalReviews;
        UpdatedAt = updatedAt;
    }

    public string? CompanyId { get; }
    public decimal OldRating { get; }
    public decimal NewRating { get; }
    public int TotalReviews { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 7. Şirket doğrulandı event'i
/// </summary>
public class CompanyVerifiedEvent : DomainEventBase
{
    public CompanyVerifiedEvent
    (
        string? companyId
        , string verificationMethod
        , string verifiedBy
        , DateTime verifiedAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        CompanyId = companyId;
        VerificationMethod = verificationMethod;
        VerifiedBy = verifiedBy;
        VerifiedAt = verifiedAt;
        Metadata = metadata;
    }

    public string? CompanyId { get; }
    public string VerificationMethod { get; }
    public string VerifiedBy { get; }
    public DateTime VerifiedAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// 8. Şirket birleşti event'i
/// </summary>
public class CompanyMergedEvent : DomainEventBase
{
    public CompanyMergedEvent
    (
        string? sourceCompanyId
        , string targetCompanyId
        , DateTime mergedAt
        , string? mergedBy = null
    ) : base()
    {
        SourceCompanyId = sourceCompanyId;
        TargetCompanyId = targetCompanyId;
        MergedAt = mergedAt;
        MergedBy = mergedBy;
    }

    public string? SourceCompanyId { get; }
    public string TargetCompanyId { get; }
    public DateTime MergedAt { get; }
    public string? MergedBy { get; }
}

/// <summary>
/// 9. Şirkete şube eklendi event'i
/// </summary>
public class CompanyBranchAddedEvent : DomainEventBase
{
    public CompanyBranchAddedEvent
    (
        string? companyId
        , string branchId
        , string branchName
        , string city
        , bool isHeadquarters
        , DateTime addedAt
    ) : base()
    {
        CompanyId = companyId;
        BranchId = branchId;
        BranchName = branchName;
        City = city;
        IsHeadquarters = isHeadquarters;
        AddedAt = addedAt;
    }

    public string? CompanyId { get; }
    public string BranchId { get; }
    public string BranchName { get; }
    public string City { get; }
    public bool IsHeadquarters { get; }
    public DateTime AddedAt { get; }
}

/// <summary>
/// 10. Şirket çalışan bilgileri güncellendi event'i
/// </summary>
public class CompanyEmployeeInfoUpdatedEvent : DomainEventBase
{
    public CompanyEmployeeInfoUpdatedEvent
    (
        string? companyId
        , int? oldEmployeeCount
        , int? newEmployeeCount
        , string? employeeCountRange
        , string companySize
        , DateTime updatedAt
    ) : base()
    {
        CompanyId = companyId;
        OldEmployeeCount = oldEmployeeCount;
        NewEmployeeCount = newEmployeeCount;
        EmployeeCountRange = employeeCountRange;
        CompanySize = companySize;
        UpdatedAt = updatedAt;
    }

    public string? CompanyId { get; }
    public int? OldEmployeeCount { get; }
    public int? NewEmployeeCount { get; }
    public string? EmployeeCountRange { get; }
    public string CompanySize { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 11. Şirket risk skoru güncellendi event'i
/// </summary>
public class CompanyRiskScoreUpdatedEvent : DomainEventBase
{
    public CompanyRiskScoreUpdatedEvent
    (
        string? companyId
        , decimal? oldScore
        , decimal newScore
        , string riskLevel
        , List<string> riskFactors
        , DateTime updatedAt
    ) : base()
    {
        CompanyId = companyId;
        OldScore = oldScore;
        NewScore = newScore;
        RiskLevel = riskLevel;
        RiskFactors = riskFactors;
        UpdatedAt = updatedAt;
    }

    public string? CompanyId { get; }
    public decimal? OldScore { get; }
    public decimal NewScore { get; }
    public string RiskLevel { get; }
    public List<string> RiskFactors { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 12. Şirket finansal bilgileri güncellendi event'i
/// </summary>
public class CompanyFinancialInfoUpdatedEvent : DomainEventBase
{
    public CompanyFinancialInfoUpdatedEvent
    (
        string? companyId
        , string? revenueRange
        , string? fundingStage
        , decimal? marketCap
        , string updatedBy
        , DateTime updatedAt
    ) : base()
    {
        CompanyId = companyId;
        RevenueRange = revenueRange;
        FundingStage = fundingStage;
        MarketCap = marketCap;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }

    public string? CompanyId { get; }
    public string? RevenueRange { get; }
    public string? FundingStage { get; }
    public decimal? MarketCap { get; }
    public string UpdatedBy { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 13. Şirket çalışma bilgileri güncellendi event'i
/// </summary>
public class CompanyWorkInfoUpdatedEvent : DomainEventBase
{
    public CompanyWorkInfoUpdatedEvent
    (
        string? companyId
        , bool? hasRemoteWork
        , string? remoteWorkPolicy
        , List<string>? benefits
        , Dictionary<string, string>? workingHours
        , DateTime updatedAt
    ) : base()
    {
        CompanyId = companyId;
        HasRemoteWork = hasRemoteWork;
        RemoteWorkPolicy = remoteWorkPolicy;
        Benefits = benefits;
        WorkingHours = workingHours;
        UpdatedAt = updatedAt;
    }

    public string? CompanyId { get; }
    public bool? HasRemoteWork { get; }
    public string? RemoteWorkPolicy { get; }
    public List<string>? Benefits { get; }
    public Dictionary<string, string>? WorkingHours { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 14. Şirket deaktif edildi event'i
/// </summary>
public class CompanyDeactivatedEvent : DomainEventBase
{
    public CompanyDeactivatedEvent
    (
        string? companyId
        , string deactivatedBy
        , string reason
        , DateTime deactivatedAt
    ) : base()
    {
        CompanyId = companyId;
        DeactivatedBy = deactivatedBy;
        Reason = reason;
        DeactivatedAt = deactivatedAt;
    }

    public string? CompanyId { get; }
    public string DeactivatedBy { get; }
    public string Reason { get; }
    public DateTime DeactivatedAt { get; }
}

/// <summary>
/// 15. Şirket yeniden aktif edildi event'i
/// </summary>
public class CompanyReactivatedEvent : DomainEventBase
{
    public CompanyReactivatedEvent
    (
        string? companyId
        , string reactivatedBy
        , string? notes
        , DateTime reactivatedAt
    ) : base()
    {
        CompanyId = companyId;
        ReactivatedBy = reactivatedBy;
        Notes = notes;
        ReactivatedAt = reactivatedAt;
    }

    public string? CompanyId { get; }
    public string ReactivatedBy { get; }
    public string? Notes { get; }
    public DateTime ReactivatedAt { get; }
}

/// <summary>
/// 16. Şirket görsel içeriği güncellendi event'i
/// </summary>
public class CompanyVisualContentUpdatedEvent : DomainEventBase
{
    public CompanyVisualContentUpdatedEvent
    (
        string? companyId
        , string? logoUrl
        , string? coverImageUrl
        , List<string>? galleryImages
        , string? companyVideo
        , DateTime updatedAt
    ) : base()
    {
        CompanyId = companyId;
        LogoUrl = logoUrl;
        CoverImageUrl = coverImageUrl;
        GalleryImages = galleryImages;
        CompanyVideo = companyVideo;
        UpdatedAt = updatedAt;
    }

    public string? CompanyId { get; }
    public string? LogoUrl { get; }
    public string? CoverImageUrl { get; }
    public List<string>? GalleryImages { get; }
    public string? CompanyVideo { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 17. Şirket sosyal medya linkleri güncellendi event'i
/// </summary>
public class CompanySocialMediaUpdatedEvent : DomainEventBase
{
    public CompanySocialMediaUpdatedEvent
    (
        string? companyId
        , string? linkedInUrl
        , string? xUrl
        , string? instagramUrl
        , string? facebookUrl
        , string? youTubeUrl
        , DateTime updatedAt
    ) : base()
    {
        CompanyId = companyId;
        LinkedInUrl = linkedInUrl;
        XUrl = xUrl;
        InstagramUrl = instagramUrl;
        FacebookUrl = facebookUrl;
        YouTubeUrl = youTubeUrl;
        UpdatedAt = updatedAt;
    }

    public string? CompanyId { get; }
    public string? LinkedInUrl { get; }
    public string? XUrl { get; }
    public string? InstagramUrl { get; }
    public string? FacebookUrl { get; }
    public string? YouTubeUrl { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 18. Şirket detaylı adres güncellendi event'i
/// </summary>
public class CompanyAddressUpdatedEvent : DomainEventBase
{
    public CompanyAddressUpdatedEvent
    (
        string? companyId
        , string address
        , string? addressLine2
        , string city
        , string? district
        , string? postalCode
        , string country
        , DateTime updatedAt
    ) : base()
    {
        CompanyId = companyId;
        Address = address;
        AddressLine2 = addressLine2;
        City = city;
        District = district;
        PostalCode = postalCode;
        Country = country;
        UpdatedAt = updatedAt;
    }

    public string? CompanyId { get; }
    public string Address { get; }
    public string? AddressLine2 { get; }
    public string City { get; }
    public string? District { get; }
    public string? PostalCode { get; }
    public string Country { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 19. Şirket iletişim bilgileri güncellendi event'i
/// </summary>
public class CompanyContactInfoUpdatedEvent : DomainEventBase
{
    public CompanyContactInfoUpdatedEvent
    (
        string? companyId
        , string phoneNumber
        , string email
        , string? hrEmail
        , string? supportEmail
        , string? faxNumber
        , DateTime updatedAt
    ) : base()
    {
        CompanyId = companyId;
        PhoneNumber = phoneNumber;
        Email = email;
        HrEmail = hrEmail;
        SupportEmail = supportEmail;
        FaxNumber = faxNumber;
        UpdatedAt = updatedAt;
    }

    public string? CompanyId { get; }
    public string PhoneNumber { get; }
    public string Email { get; }
    public string? HrEmail { get; }
    public string? SupportEmail { get; }
    public string? FaxNumber { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 20. Şirket ana şirket ilişkisi kuruldu event'i
/// </summary>
public class CompanyParentSetEvent : DomainEventBase
{
    public CompanyParentSetEvent
    (
        string? companyId
        , string parentCompanyId
        , string setBy
        , DateTime setAt
    ) : base()
    {
        CompanyId = companyId;
        ParentCompanyId = parentCompanyId;
        SetBy = setBy;
        SetAt = setAt;
    }

    public string? CompanyId { get; }
    public string ParentCompanyId { get; }
    public string SetBy { get; }
    public DateTime SetAt { get; }
}

/// <summary>
/// 21. Şirkete bağlı şirket eklendi event'i
/// </summary>
public class CompanySubsidiaryAddedEvent : DomainEventBase
{
    public CompanySubsidiaryAddedEvent
    (
        string? companyId
        , string subsidiaryId
        , string addedBy
        , DateTime addedAt
    ) : base()
    {
        CompanyId = companyId;
        SubsidiaryId = subsidiaryId;
        AddedBy = addedBy;
        AddedAt = addedAt;
    }

    public string? CompanyId { get; }
    public string SubsidiaryId { get; }
    public string AddedBy { get; }
    public DateTime AddedAt { get; }
}

/// <summary>
/// 22. Şirket ID'si güncellendi event'i
/// </summary>
public class CompanyIdUpdatedEvent : DomainEventBase
{
    public CompanyIdUpdatedEvent
    (
        string oldCompanyId
        , string newCompanyId
        , DateTime updatedAt
    ) : base()
    {
        OldCompanyId = oldCompanyId;
        NewCompanyId = newCompanyId;
        UpdatedAt = updatedAt;
    }

    public string OldCompanyId { get; }
    public string NewCompanyId { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 23. Şirket görüntülendi event'i
/// </summary>
public class CompanyViewedEvent : DomainEventBase
{
    public CompanyViewedEvent
    (
        string? companyId
        , string? viewedByUserId
        , string viewerIp
        , string? referrerUrl
        , string userAgent
        , DateTime viewedAt
    ) : base()
    {
        CompanyId = companyId;
        ViewedByUserId = viewedByUserId;
        ViewerIp = viewerIp;
        ReferrerUrl = referrerUrl;
        UserAgent = userAgent;
        ViewedAt = viewedAt;
    }

    public string? CompanyId { get; }
    public string? ViewedByUserId { get; }
    public string ViewerIp { get; }
    public string? ReferrerUrl { get; }
    public string UserAgent { get; }
    public DateTime ViewedAt { get; }
}

/// <summary>
/// 24. Şirket arandı event'i
/// </summary>
public class CompanySearchedEvent : DomainEventBase
{
    public CompanySearchedEvent
    (
        string searchQuery
        , string? sector
        , string? city
        , int resultCount
        , string? searchByUserId
        , string searcherIp
        , Dictionary<string, object>? searchFilters
        , DateTime searchedAt
    ) : base()
    {
        SearchQuery = searchQuery;
        Sector = sector;
        City = city;
        ResultCount = resultCount;
        SearchByUserId = searchByUserId;
        SearcherIp = searcherIp;
        SearchFilters = searchFilters;
        SearchedAt = searchedAt;
    }

    public string SearchQuery { get; }
    public string? Sector { get; }
    public string? City { get; }
    public int ResultCount { get; }
    public string? SearchByUserId { get; }
    public string SearcherIp { get; }
    public Dictionary<string, object>? SearchFilters { get; }
    public DateTime SearchedAt { get; }
}

/// <summary>
/// 25. Şirket geçici olarak askıya alındı event'i
/// </summary>
public class CompanySuspendedEvent : DomainEventBase
{
    public CompanySuspendedEvent
    (
        string? companyId
        , string suspendedBy
        , string reason
        , DateTime? suspensionEndDate
        , bool canAppeal
        , DateTime suspendedAt
    ) : base()
    {
        CompanyId = companyId;
        SuspendedBy = suspendedBy;
        Reason = reason;
        SuspensionEndDate = suspensionEndDate;
        CanAppeal = canAppeal;
        SuspendedAt = suspendedAt;
    }

    public string? CompanyId { get; }
    public string SuspendedBy { get; }
    public string Reason { get; }
    public DateTime? SuspensionEndDate { get; }
    public bool CanAppeal { get; }
    public DateTime SuspendedAt { get; }
}

/// <summary>
/// 26. Şirket takip edildi event'i
/// </summary>
public class CompanyFollowedEvent : DomainEventBase
{
    public CompanyFollowedEvent
    (
        string? companyId
        , string userId
        , bool enableNotifications
        , DateTime followedAt
    ) : base()
    {
        CompanyId = companyId;
        UserId = userId;
        EnableNotifications = enableNotifications;
        FollowedAt = followedAt;
    }

    public string? CompanyId { get; }
    public string UserId { get; }
    public bool EnableNotifications { get; }
    public DateTime FollowedAt { get; }
}

/// <summary>
/// 27. Şirket takibi bırakıldı event'i
/// </summary>
public class CompanyUnfollowedEvent : DomainEventBase
{
    public CompanyUnfollowedEvent
    (
        string? companyId
        , string userId
        , string? unfollowReason
        , DateTime unfollowedAt
    ) : base()
    {
        CompanyId = companyId;
        UserId = userId;
        UnfollowReason = unfollowReason;
        UnfollowedAt = unfollowedAt;
    }

    public string? CompanyId { get; }
    public string UserId { get; }
    public string? UnfollowReason { get; }
    public DateTime UnfollowedAt { get; }
}

/// <summary>
/// 28. Şirket sahibi tarafından talep edildi event'i
/// </summary>
public class CompanyClaimedEvent : DomainEventBase
{
    public CompanyClaimedEvent
    (
        string? companyId
        , string claimedByUserId
        , string claimantEmail
        , string? verificationDocumentUrl
        , string claimStatus
        , DateTime claimedAt
    ) : base()
    {
        CompanyId = companyId;
        ClaimedByUserId = claimedByUserId;
        ClaimantEmail = claimantEmail;
        VerificationDocumentUrl = verificationDocumentUrl;
        ClaimStatus = claimStatus;
        ClaimedAt = claimedAt;
    }

    public string? CompanyId { get; }
    public string ClaimedByUserId { get; }
    public string ClaimantEmail { get; }
    public string? VerificationDocumentUrl { get; }
    public string ClaimStatus { get; }
    public DateTime ClaimedAt { get; }
}
