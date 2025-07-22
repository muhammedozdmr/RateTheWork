namespace RateTheWork.Domain.Events.JobPosting;

/// <summary>
/// İş ilanı oluşturuldu eventi
/// </summary>
public class JobPostingCreatedEvent : DomainEventBase
{
    public JobPostingCreatedEvent
    (
        string jobPostingId
        , string companyId
        , string hrPersonnelId
        , string title
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        JobPostingId = jobPostingId;
        CompanyId = companyId;
        HRPersonnelId = hrPersonnelId;
        Title = title;
        Metadata = metadata;
    }

    public string JobPostingId { get; }
    public string CompanyId { get; }
    public string HRPersonnelId { get; }
    public string Title { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İş ilanı yayınlandı eventi
/// </summary>
public class JobPostingPublishedEvent : DomainEventBase
{
    public JobPostingPublishedEvent
    (
        string jobPostingId
        , string companyId
        , string title
        , DateTime publishedAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        JobPostingId = jobPostingId;
        CompanyId = companyId;
        Title = title;
        PublishedAt = publishedAt;
        Metadata = metadata;
    }

    public string JobPostingId { get; }
    public string CompanyId { get; }
    public string Title { get; }
    public DateTime PublishedAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İş ilanı hedef başvuru sayısına ulaştı eventi
/// </summary>
public class JobPostingTargetReachedEvent : DomainEventBase
{
    public JobPostingTargetReachedEvent
    (
        string jobPostingId
        , string companyId
        , string title
        , int targetCount
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        JobPostingId = jobPostingId;
        CompanyId = companyId;
        Title = title;
        TargetCount = targetCount;
        Metadata = metadata;
    }

    public string JobPostingId { get; }
    public string CompanyId { get; }
    public string Title { get; }
    public int TargetCount { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İş ilanı tamamlandı eventi
/// </summary>
public class JobPostingCompletedEvent : DomainEventBase
{
    public JobPostingCompletedEvent
    (
        string jobPostingId
        , string companyId
        , string title
        , int totalApplications
        , int hiredCount
        , DateTime completedAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        JobPostingId = jobPostingId;
        CompanyId = companyId;
        Title = title;
        TotalApplications = totalApplications;
        HiredCount = hiredCount;
        CompletedAt = completedAt;
        Metadata = metadata;
    }

    public string JobPostingId { get; }
    public string CompanyId { get; }
    public string Title { get; }
    public int TotalApplications { get; }
    public int HiredCount { get; }
    public DateTime CompletedAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İş ilanı iptal edildi eventi
/// </summary>
public class JobPostingCancelledEvent : DomainEventBase
{
    public JobPostingCancelledEvent
    (
        string jobPostingId
        , string companyId
        , string title
        , string reason
        , DateTime cancelledAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        JobPostingId = jobPostingId;
        CompanyId = companyId;
        Title = title;
        Reason = reason;
        CancelledAt = cancelledAt;
        Metadata = metadata;
    }

    public string JobPostingId { get; }
    public string CompanyId { get; }
    public string Title { get; }
    public string Reason { get; }
    public DateTime CancelledAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İş ilanı süresi doldu eventi
/// </summary>
public class JobPostingExpiredEvent : DomainEventBase
{
    public JobPostingExpiredEvent
    (
        string jobPostingId
        , string companyId
        , string title
        , int totalApplications
        , DateTime expiredAt
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        JobPostingId = jobPostingId;
        CompanyId = companyId;
        Title = title;
        TotalApplications = totalApplications;
        ExpiredAt = expiredAt;
        Metadata = metadata;
    }

    public string JobPostingId { get; }
    public string CompanyId { get; }
    public string Title { get; }
    public int TotalApplications { get; }
    public DateTime ExpiredAt { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Şüpheli havuz ilanı tespit edildi eventi
/// </summary>
public class SuspiciousPoolJobDetectedEvent : DomainEventBase
{
    public SuspiciousPoolJobDetectedEvent
    (
        string jobPostingId
        , string companyId
        , string hrPersonnelId
        , string title
        , string[] reasons
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        JobPostingId = jobPostingId;
        CompanyId = companyId;
        HRPersonnelId = hrPersonnelId;
        Title = title;
        Reasons = reasons;
        Metadata = metadata;
    }

    public string JobPostingId { get; }
    public string CompanyId { get; }
    public string HRPersonnelId { get; }
    public string Title { get; }
    public string[] Reasons { get; }
    public Dictionary<string, object>? Metadata { get; }
}
