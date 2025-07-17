namespace RateTheWork.Domain.Events.CompanyBranch;

/// <summary>
/// 1. Şirket şubesi oluşturuldu event'i
/// </summary>
public record CompanyBranchCreatedEvent(
    string? BranchId
    , string CompanyId
    , string BranchName
    , string BranchType
    , string Address
    , string City
    , string? District
    , string? PostalCode
    , string Country
    , string? PhoneNumber
    , string? Email
    , bool IsHeadquarters
    , string CreatedBy
    , DateTime CreatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 2. Şirket şubesi güncellendi event'i
/// </summary>
public record CompanyBranchUpdatedEvent(
    string? BranchId
    , string CompanyId
    , string[] UpdatedFields
    , Dictionary<string, object>? OldValues
    , Dictionary<string, object>? NewValues
    , string UpdatedBy
    , DateTime UpdatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 3. Şirket şubesi merkez olarak belirlendi event'i
/// </summary>
public record CompanyBranchSetAsHeadquartersEvent(
    string? BranchId
    , string CompanyId
    , string? PreviousHeadquartersBranchId
    , string SetBy
    , DateTime SetAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 4. Şirket şubesi silindi event'i
/// </summary>
public record CompanyBranchDeletedEvent(
    string? BranchId
    , string CompanyId
    , string DeletedBy
    , string DeletionReason
    , bool IsHardDelete
    , DateTime DeletedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 5. Şirket şubesi deaktif edildi event'i
/// </summary>
public record CompanyBranchDeactivatedEvent(
    string? BranchId
    , string CompanyId
    , string DeactivatedBy
    , string Reason
    , DateTime? ReactivationDate
    , DateTime DeactivatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}

/// <summary>
/// 6. Şirket şubesi yeniden aktif edildi event'i
/// </summary>
public record CompanyBranchReactivatedEvent(
    string? BranchId
    , string CompanyId
    , string ReactivatedBy
    , DateTime ReactivatedAt
    , DateTime OccurredOn = default
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = OccurredOn == default ? DateTime.UtcNow : OccurredOn;
}
