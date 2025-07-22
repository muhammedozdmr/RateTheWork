namespace RateTheWork.Domain.Events.CompanyBranch;

/// <summary>
/// 1. Şirket şubesi oluşturuldu event'i
/// </summary>
public class CompanyBranchCreatedEvent : DomainEventBase
{
    public CompanyBranchCreatedEvent
    (
        string? branchId
        , string companyId
        , string branchName
        , string branchType
        , string address
        , string city
        , string? district
        , string? postalCode
        , string country
        , string? phoneNumber
        , string? email
        , bool isHeadquarters
        , string createdBy
        , DateTime createdAt
    ) : base()
    {
        BranchId = branchId;
        CompanyId = companyId;
        BranchName = branchName;
        BranchType = branchType;
        Address = address;
        City = city;
        District = district;
        PostalCode = postalCode;
        Country = country;
        PhoneNumber = phoneNumber;
        Email = email;
        IsHeadquarters = isHeadquarters;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }

    public string? BranchId { get; }
    public string CompanyId { get; }
    public string BranchName { get; }
    public string BranchType { get; }
    public string Address { get; }
    public string City { get; }
    public string? District { get; }
    public string? PostalCode { get; }
    public string Country { get; }
    public string? PhoneNumber { get; }
    public string? Email { get; }
    public bool IsHeadquarters { get; }
    public string CreatedBy { get; }
    public DateTime CreatedAt { get; }
}

/// <summary>
/// 2. Şirket şubesi güncellendi event'i
/// </summary>
public class CompanyBranchUpdatedEvent : DomainEventBase
{
    public CompanyBranchUpdatedEvent
    (
        string? branchId
        , string companyId
        , string[] updatedFields
        , Dictionary<string, object>? oldValues
        , Dictionary<string, object>? newValues
        , string updatedBy
        , DateTime updatedAt
    ) : base()
    {
        BranchId = branchId;
        CompanyId = companyId;
        UpdatedFields = updatedFields;
        OldValues = oldValues;
        NewValues = newValues;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }

    public string? BranchId { get; }
    public string CompanyId { get; }
    public string[] UpdatedFields { get; }
    public Dictionary<string, object>? OldValues { get; }
    public Dictionary<string, object>? NewValues { get; }
    public string UpdatedBy { get; }
    public DateTime UpdatedAt { get; }
}

/// <summary>
/// 3. Şirket şubesi merkez olarak belirlendi event'i
/// </summary>
public class CompanyBranchSetAsHeadquartersEvent : DomainEventBase
{
    public CompanyBranchSetAsHeadquartersEvent
    (
        string? branchId
        , string companyId
        , string? previousHeadquartersBranchId
        , string setBy
        , DateTime setAt
    ) : base()
    {
        BranchId = branchId;
        CompanyId = companyId;
        PreviousHeadquartersBranchId = previousHeadquartersBranchId;
        SetBy = setBy;
        SetAt = setAt;
    }

    public string? BranchId { get; }
    public string CompanyId { get; }
    public string? PreviousHeadquartersBranchId { get; }
    public string SetBy { get; }
    public DateTime SetAt { get; }
}

/// <summary>
/// 4. Şirket şubesi silindi event'i
/// </summary>
public class CompanyBranchDeletedEvent : DomainEventBase
{
    public CompanyBranchDeletedEvent
    (
        string? branchId
        , string companyId
        , string deletedBy
        , string deletionReason
        , bool isHardDelete
        , DateTime deletedAt
    ) : base()
    {
        BranchId = branchId;
        CompanyId = companyId;
        DeletedBy = deletedBy;
        DeletionReason = deletionReason;
        IsHardDelete = isHardDelete;
        DeletedAt = deletedAt;
    }

    public string? BranchId { get; }
    public string CompanyId { get; }
    public string DeletedBy { get; }
    public string DeletionReason { get; }
    public bool IsHardDelete { get; }
    public DateTime DeletedAt { get; }
}

/// <summary>
/// 5. Şirket şubesi deaktif edildi event'i
/// </summary>
public class CompanyBranchDeactivatedEvent : DomainEventBase
{
    public CompanyBranchDeactivatedEvent
    (
        string? branchId
        , string companyId
        , string deactivatedBy
        , string reason
        , DateTime? reactivationDate
        , DateTime deactivatedAt
    ) : base()
    {
        BranchId = branchId;
        CompanyId = companyId;
        DeactivatedBy = deactivatedBy;
        Reason = reason;
        ReactivationDate = reactivationDate;
        DeactivatedAt = deactivatedAt;
    }

    public string? BranchId { get; }
    public string CompanyId { get; }
    public string DeactivatedBy { get; }
    public string Reason { get; }
    public DateTime? ReactivationDate { get; }
    public DateTime DeactivatedAt { get; }
}

/// <summary>
/// 6. Şirket şubesi yeniden aktif edildi event'i
/// </summary>
public class CompanyBranchReactivatedEvent : DomainEventBase
{
    public CompanyBranchReactivatedEvent
    (
        string? branchId
        , string companyId
        , string reactivatedBy
        , DateTime reactivatedAt
    ) : base()
    {
        BranchId = branchId;
        CompanyId = companyId;
        ReactivatedBy = reactivatedBy;
        ReactivatedAt = reactivatedAt;
    }

    public string? BranchId { get; }
    public string CompanyId { get; }
    public string ReactivatedBy { get; }
    public DateTime ReactivatedAt { get; }
}
