namespace RateTheWork.Domain.Events.Company;

/// <summary>
/// 1. Şirket oluşturuldu event'i
/// </summary>
public record CompanyCreatedEvent(
    string? CompanyId,
    string Name,
    string TaxId,
    string MersisNo,
    string Sector,
    DateTime CreatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Şirket onaylandı event'i
/// </summary>
public record CompanyApprovedEvent(
    string? CompanyId,
    string ApprovedBy,
    string? Notes,
    DateTime ApprovedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 3. Şirket reddedildi event'i
/// </summary>
public record CompanyRejectedEvent(
    string? CompanyId,
    string RejectedBy,
    string Reason,
    DateTime RejectedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 4. Şirket bilgileri güncellendi event'i
/// </summary>
public record CompanyInfoUpdatedEvent(
    string? CompanyId,
    string[] UpdatedFields,
    string UpdatedBy,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 5. Şirket logosu güncellendi event'i
/// </summary>
public record CompanyLogoUpdatedEvent(
    string? CompanyId,
    string LogoUrl,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 6. Şirket puanı güncellendi event'i
/// </summary>
public record CompanyRatingUpdatedEvent(
    string? CompanyId,
    decimal OldRating,
    decimal NewRating,
    int TotalReviews,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 7. Şirket doğrulandı event'i
/// </summary>
public record CompanyVerifiedEvent(
    string? CompanyId,
    string VerificationMethod,
    string VerifiedBy,
    DateTime VerifiedAt,
    Dictionary<string, object>? Metadata = null,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 8. Şirket birleşti event'i
/// </summary>
public record CompanyMergedEvent(
    string? SourceCompanyId,
    string TargetCompanyId,
    DateTime MergedAt,
    string? MergedBy = null,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 9. Şirkete şube eklendi event'i
/// </summary>
public record CompanyBranchAddedEvent(
    string? CompanyId,
    string BranchId,
    string BranchName,
    string City,
    bool IsHeadquarters,
    DateTime AddedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 10. Şirket çalışan bilgileri güncellendi event'i
/// </summary>
public record CompanyEmployeeInfoUpdatedEvent(
    string? CompanyId,
    int? OldEmployeeCount,
    int? NewEmployeeCount,
    string? EmployeeCountRange,
    string CompanySize,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 11. Şirket risk skoru güncellendi event'i
/// </summary>
public record CompanyRiskScoreUpdatedEvent(
    string? CompanyId,
    decimal? OldScore,
    decimal NewScore,
    string RiskLevel,
    List<string> RiskFactors,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 12. Şirket finansal bilgileri güncellendi event'i
/// </summary>
public record CompanyFinancialInfoUpdatedEvent(
    string? CompanyId,
    string? RevenueRange,
    string? FundingStage,
    decimal? MarketCap,
    string UpdatedBy,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 13. Şirket çalışma bilgileri güncellendi event'i
/// </summary>
public record CompanyWorkInfoUpdatedEvent(
    string? CompanyId,
    bool? HasRemoteWork,
    string? RemoteWorkPolicy,
    List<string>? Benefits,
    Dictionary<string, string>? WorkingHours,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 14. Şirket deaktif edildi event'i
/// </summary>
public record CompanyDeactivatedEvent(
    string? CompanyId,
    string DeactivatedBy,
    string Reason,
    DateTime DeactivatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 15. Şirket yeniden aktif edildi event'i
/// </summary>
public record CompanyReactivatedEvent(
    string? CompanyId,
    string ReactivatedBy,
    string? Notes,
    DateTime ReactivatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 16. Şirket görsel içeriği güncellendi event'i
/// </summary>
public record CompanyVisualContentUpdatedEvent(
    string? CompanyId,
    string? LogoUrl,
    string? CoverImageUrl,
    List<string>? GalleryImages,
    string? CompanyVideo,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 17. Şirket sosyal medya linkleri güncellendi event'i
/// </summary>
public record CompanySocialMediaUpdatedEvent(
    string? CompanyId,
    string? LinkedInUrl,
    string? XUrl,
    string? InstagramUrl,
    string? FacebookUrl,
    string? YouTubeUrl,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 18. Şirket detaylı adres güncellendi event'i
/// </summary>
public record CompanyAddressUpdatedEvent(
    string? CompanyId,
    string Address,
    string? AddressLine2,
    string City,
    string? District,
    string? PostalCode,
    string Country,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 19. Şirket iletişim bilgileri güncellendi event'i
/// </summary>
public record CompanyContactInfoUpdatedEvent(
    string? CompanyId,
    string PhoneNumber,
    string Email,
    string? HrEmail,
    string? SupportEmail,
    string? FaxNumber,
    DateTime UpdatedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 20. Şirket ana şirket ilişkisi kuruldu event'i
/// </summary>
public record CompanyParentSetEvent(
    string? CompanyId,
    string ParentCompanyId,
    string SetBy,
    DateTime SetAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 21. Şirkete bağlı şirket eklendi event'i
/// </summary>
public record CompanySubsidiaryAddedEvent(
    string? CompanyId,
    string SubsidiaryId,
    string AddedBy,
    DateTime AddedAt,
    DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
