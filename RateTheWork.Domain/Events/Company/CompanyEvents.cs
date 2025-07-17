namespace RateTheWork.Domain.Events.Company;

/// <summary>
/// 1. Şirket oluşturuldu event'i
/// </summary>
public record CompanyCreatedEvent(
    string? CompanyId
    , string Name
    , string TaxId
    , string MersisNo
    , string CompanyType
    , string Sector
    , string City
    , string? Address
    , string? PhoneNumber
    , string? Email
    , string? WebsiteUrl
    , int? EstablishedYear
    , string? CreatedByUserId
    , string CreatedByIp
    , DateTime CreatedAt
) : DomainEventBase;

/// <summary>
/// 2. Şirket onaylandı event'i
/// </summary>
public record CompanyApprovedEvent(
    string? CompanyId
    , string ApprovedBy
    , string? Notes
    , DateTime ApprovedAt
) : DomainEventBase;

/// <summary>
/// 3. Şirket reddedildi event'i
/// </summary>
public record CompanyRejectedEvent(
    string? CompanyId
    , string RejectedBy
    , string Reason
    , DateTime RejectedAt
) : DomainEventBase;

/// <summary>
/// 4. Şirket bilgileri güncellendi event'i
/// </summary>
public record CompanyInfoUpdatedEvent(
    string? CompanyId
    , string[] UpdatedFields
    , Dictionary<string, object>? OldValues
    , Dictionary<string, object>? NewValues
    , string UpdatedBy
    , string? UpdateReason
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 5. Şirket logosu güncellendi event'i
/// </summary>
public record CompanyLogoUpdatedEvent(
    string? CompanyId
    , string LogoUrl
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 6. Şirket puanı güncellendi event'i
/// </summary>
public record CompanyRatingUpdatedEvent(
    string? CompanyId
    , decimal OldRating
    , decimal NewRating
    , int TotalReviews
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 7. Şirket doğrulandı event'i
/// </summary>
public record CompanyVerifiedEvent(
    string? CompanyId
    , string VerificationMethod
    , string VerifiedBy
    , DateTime VerifiedAt
    , Dictionary<string, object>? Metadata = null
) : DomainEventBase;

/// <summary>
/// 8. Şirket birleşti event'i
/// </summary>
public record CompanyMergedEvent(
    string? SourceCompanyId
    , string TargetCompanyId
    , DateTime MergedAt
    , string? MergedBy = null
) : DomainEventBase;

/// <summary>
/// 9. Şirkete şube eklendi event'i
/// </summary>
public record CompanyBranchAddedEvent(
    string? CompanyId
    , string BranchId
    , string BranchName
    , string City
    , bool IsHeadquarters
    , DateTime AddedAt
) : DomainEventBase;

/// <summary>
/// 10. Şirket çalışan bilgileri güncellendi event'i
/// </summary>
public record CompanyEmployeeInfoUpdatedEvent(
    string? CompanyId
    , int? OldEmployeeCount
    , int? NewEmployeeCount
    , string? EmployeeCountRange
    , string CompanySize
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 11. Şirket risk skoru güncellendi event'i
/// </summary>
public record CompanyRiskScoreUpdatedEvent(
    string? CompanyId
    , decimal? OldScore
    , decimal NewScore
    , string RiskLevel
    , List<string> RiskFactors
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 12. Şirket finansal bilgileri güncellendi event'i
/// </summary>
public record CompanyFinancialInfoUpdatedEvent(
    string? CompanyId
    , string? RevenueRange
    , string? FundingStage
    , decimal? MarketCap
    , string UpdatedBy
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 13. Şirket çalışma bilgileri güncellendi event'i
/// </summary>
public record CompanyWorkInfoUpdatedEvent(
    string? CompanyId
    , bool? HasRemoteWork
    , string? RemoteWorkPolicy
    , List<string>? Benefits
    , Dictionary<string, string>? WorkingHours
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 14. Şirket deaktif edildi event'i
/// </summary>
public record CompanyDeactivatedEvent(
    string? CompanyId
    , string DeactivatedBy
    , string Reason
    , DateTime DeactivatedAt
) : DomainEventBase;

/// <summary>
/// 15. Şirket yeniden aktif edildi event'i
/// </summary>
public record CompanyReactivatedEvent(
    string? CompanyId
    , string ReactivatedBy
    , string? Notes
    , DateTime ReactivatedAt
) : DomainEventBase;

/// <summary>
/// 16. Şirket görsel içeriği güncellendi event'i
/// </summary>
public record CompanyVisualContentUpdatedEvent(
    string? CompanyId
    , string? LogoUrl
    , string? CoverImageUrl
    , List<string>? GalleryImages
    , string? CompanyVideo
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 17. Şirket sosyal medya linkleri güncellendi event'i
/// </summary>
public record CompanySocialMediaUpdatedEvent(
    string? CompanyId
    , string? LinkedInUrl
    , string? XUrl
    , string? InstagramUrl
    , string? FacebookUrl
    , string? YouTubeUrl
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 18. Şirket detaylı adres güncellendi event'i
/// </summary>
public record CompanyAddressUpdatedEvent(
    string? CompanyId
    , string Address
    , string? AddressLine2
    , string City
    , string? District
    , string? PostalCode
    , string Country
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 19. Şirket iletişim bilgileri güncellendi event'i
/// </summary>
public record CompanyContactInfoUpdatedEvent(
    string? CompanyId
    , string PhoneNumber
    , string Email
    , string? HrEmail
    , string? SupportEmail
    , string? FaxNumber
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 20. Şirket ana şirket ilişkisi kuruldu event'i
/// </summary>
public record CompanyParentSetEvent(
    string? CompanyId
    , string ParentCompanyId
    , string SetBy
    , DateTime SetAt
) : DomainEventBase;

/// <summary>
/// 21. Şirkete bağlı şirket eklendi event'i
/// </summary>
public record CompanySubsidiaryAddedEvent(
    string? CompanyId
    , string SubsidiaryId
    , string AddedBy
    , DateTime AddedAt
) : DomainEventBase;

/// <summary>
/// 22. Şirket ID'si güncellendi event'i
/// </summary>
public record CompanyIdUpdatedEvent(
    string OldCompanyId
    , string NewCompanyId
    , DateTime UpdatedAt
) : DomainEventBase;

/// <summary>
/// 23. Şirket görüntülendi event'i
/// </summary>
public record CompanyViewedEvent(
    string? CompanyId
    , string? ViewedByUserId
    , string ViewerIp
    , string? ReferrerUrl
    , string UserAgent
    , DateTime ViewedAt
) : DomainEventBase;

/// <summary>
/// 24. Şirket arandı event'i
/// </summary>
public record CompanySearchedEvent(
    string SearchQuery
    , string? Sector
    , string? City
    , int ResultCount
    , string? SearchByUserId
    , string SearcherIp
    , Dictionary<string, object>? SearchFilters
    , DateTime SearchedAt
) : DomainEventBase;

/// <summary>
/// 25. Şirket geçici olarak askıya alındı event'i
/// </summary>
public record CompanySuspendedEvent(
    string? CompanyId
    , string SuspendedBy
    , string Reason
    , DateTime? SuspensionEndDate
    , bool CanAppeal
    , DateTime SuspendedAt
) : DomainEventBase;

/// <summary>
/// 26. Şirket takip edildi event'i
/// </summary>
public record CompanyFollowedEvent(
    string? CompanyId
    , string UserId
    , bool EnableNotifications
    , DateTime FollowedAt
) : DomainEventBase;

/// <summary>
/// 27. Şirket takibi bırakıldı event'i
/// </summary>
public record CompanyUnfollowedEvent(
    string? CompanyId
    , string UserId
    , string? UnfollowReason
    , DateTime UnfollowedAt
) : DomainEventBase;

/// <summary>
/// 28. Şirket sahibi tarafından talep edildi event'i
/// </summary>
public record CompanyClaimedEvent(
    string? CompanyId
    , string ClaimedByUserId
    , string ClaimantEmail
    , string? VerificationDocumentUrl
    , string ClaimStatus
    , DateTime ClaimedAt
) : DomainEventBase;
