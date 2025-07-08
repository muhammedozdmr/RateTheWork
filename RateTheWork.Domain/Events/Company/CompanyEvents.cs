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
    string CompanyId,
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
    string CompanyId,
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
    string CompanyId,
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
    string CompanyId,
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
