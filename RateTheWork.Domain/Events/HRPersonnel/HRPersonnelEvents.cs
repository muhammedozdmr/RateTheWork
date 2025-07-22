namespace RateTheWork.Domain.Events.HRPersonnel;

/// <summary>
/// İK personeli oluşturuldu eventi
/// </summary>
public class HRPersonnelCreatedEvent : DomainEventBase
{
    public HRPersonnelCreatedEvent
    (
        string personnelId
        , string userId
        , string companyId
        , string fullName
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        PersonnelId = personnelId;
        UserId = userId;
        CompanyId = companyId;
        FullName = fullName;
        Metadata = metadata;
    }

    public string PersonnelId { get; }
    public string UserId { get; }
    public string CompanyId { get; }
    public string FullName { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İK personeli doğrulandı eventi
/// </summary>
public class HRPersonnelVerifiedEvent : DomainEventBase
{
    public HRPersonnelVerifiedEvent
    (
        string personnelId
        , string companyId
        , string fullName
        , DateTime verifiedAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        PersonnelId = personnelId;
        CompanyId = companyId;
        FullName = fullName;
        VerifiedAt = verifiedAt;
        Metadata = metadata;
    }

    public string PersonnelId { get; }
    public string CompanyId { get; }
    public string FullName { get; }
    public DateTime VerifiedAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İK personeli deaktif edildi eventi
/// </summary>
public class HRPersonnelDeactivatedEvent : DomainEventBase
{
    public HRPersonnelDeactivatedEvent
    (
        string personnelId
        , string companyId
        , string fullName
        , DateTime deactivatedAt
        , string reason
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        PersonnelId = personnelId;
        CompanyId = companyId;
        FullName = fullName;
        DeactivatedAt = deactivatedAt;
        Reason = reason;
        Metadata = metadata;
    }

    public string PersonnelId { get; }
    public string CompanyId { get; }
    public string FullName { get; }
    public DateTime DeactivatedAt { get; }
    public string Reason { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İK personeli güven skoru güncellendi eventi
/// </summary>
public class HRPersonnelTrustScoreUpdatedEvent : DomainEventBase
{
    public HRPersonnelTrustScoreUpdatedEvent
    (
        string personnelId
        , string companyId
        , decimal oldScore
        , decimal newScore
        , string reason
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        PersonnelId = personnelId;
        CompanyId = companyId;
        OldScore = oldScore;
        NewScore = newScore;
        Reason = reason;
        Metadata = metadata;
    }

    public string PersonnelId { get; }
    public string CompanyId { get; }
    public decimal OldScore { get; }
    public decimal NewScore { get; }
    public string Reason { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İK personeli performans değerlendirmesi eventi
/// </summary>
public class HRPersonnelPerformanceEvaluatedEvent : DomainEventBase
{
    public HRPersonnelPerformanceEvaluatedEvent
    (
        string personnelId
        , string companyId
        , decimal performanceScore
        , DateTime evaluatedAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        PersonnelId = personnelId;
        CompanyId = companyId;
        PerformanceScore = performanceScore;
        EvaluatedAt = evaluatedAt;
        Metadata = metadata;
    }

    public string PersonnelId { get; }
    public string CompanyId { get; }
    public decimal PerformanceScore { get; }
    public DateTime EvaluatedAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}
