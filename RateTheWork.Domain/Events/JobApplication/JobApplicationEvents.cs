namespace RateTheWork.Domain.Events.JobApplication;

/// <summary>
/// İş başvurusu oluşturuldu eventi
/// </summary>
public class JobApplicationCreatedEvent : DomainEventBase
{
    public JobApplicationCreatedEvent
    (
        string applicationId
        , string jobPostingId
        , string applicantUserId
        , string applicantName
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        ApplicationId = applicationId;
        JobPostingId = jobPostingId;
        ApplicantUserId = applicantUserId;
        ApplicantName = applicantName;
        Metadata = metadata;
    }

    public string ApplicationId { get; }
    public string JobPostingId { get; }
    public string ApplicantUserId { get; }
    public string ApplicantName { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Başvuran mülakata davet edildi eventi
/// </summary>
public class ApplicantInvitedToInterviewEvent : DomainEventBase
{
    public ApplicantInvitedToInterviewEvent
    (
        string applicationId
        , string jobPostingId
        , string applicantUserId
        , DateTime interviewDate
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        ApplicationId = applicationId;
        JobPostingId = jobPostingId;
        ApplicantUserId = applicantUserId;
        InterviewDate = interviewDate;
        Metadata = metadata;
    }

    public string ApplicationId { get; }
    public string JobPostingId { get; }
    public string ApplicantUserId { get; }
    public DateTime InterviewDate { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// İş teklifi yapıldı eventi
/// </summary>
public class JobOfferMadeEvent : DomainEventBase
{
    public JobOfferMadeEvent
    (
        string applicationId
        , string jobPostingId
        , string applicantUserId
        , decimal offeredSalary
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        ApplicationId = applicationId;
        JobPostingId = jobPostingId;
        ApplicantUserId = applicantUserId;
        OfferedSalary = offeredSalary;
        Metadata = metadata;
    }

    public string ApplicationId { get; }
    public string JobPostingId { get; }
    public string ApplicantUserId { get; }
    public decimal OfferedSalary { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Başvuran işe alındı eventi
/// </summary>
public class ApplicantHiredEvent : DomainEventBase
{
    public ApplicantHiredEvent
    (
        string applicationId
        , string jobPostingId
        , string applicantUserId
        , string companyId
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        ApplicationId = applicationId;
        JobPostingId = jobPostingId;
        ApplicantUserId = applicantUserId;
        CompanyId = companyId;
        Metadata = metadata;
    }

    public string ApplicationId { get; }
    public string JobPostingId { get; }
    public string ApplicantUserId { get; }
    public string CompanyId { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Başvuru reddedildi eventi
/// </summary>
public class ApplicationRejectedEvent : DomainEventBase
{
    public ApplicationRejectedEvent
    (
        string applicationId
        , string jobPostingId
        , string applicantUserId
        , string reason
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        ApplicationId = applicationId;
        JobPostingId = jobPostingId;
        ApplicantUserId = applicantUserId;
        Reason = reason;
        Metadata = metadata;
    }

    public string ApplicationId { get; }
    public string JobPostingId { get; }
    public string ApplicantUserId { get; }
    public string Reason { get; }
    public Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Başvuru geri çekildi eventi
/// </summary>
public class ApplicationWithdrawnEvent : DomainEventBase
{
    public ApplicationWithdrawnEvent
    (
        string applicationId
        , string jobPostingId
        , string applicantUserId
        , string reason
        , Dictionary<string, object>? metadata = null
    ) : base()
    {
        ApplicationId = applicationId;
        JobPostingId = jobPostingId;
        ApplicantUserId = applicantUserId;
        Reason = reason;
        Metadata = metadata;
    }

    public string ApplicationId { get; }
    public string JobPostingId { get; }
    public string ApplicantUserId { get; }
    public string Reason { get; }
    public Dictionary<string, object>? Metadata { get; }
}
