using RateTheWork.Domain.Enums.CVApplication;

namespace RateTheWork.Domain.Events.CVApplication;

using RateTheWork.Domain.Events;

/// <summary>
/// CV başvurusu gönderildi event'i
/// </summary>
public class CVApplicationSubmittedEvent : DomainEventBase
{
    public CVApplicationSubmittedEvent(
        string cvApplicationId,
        string userId,
        string companyId,
        List<string> departmentIds,
        string applicantName,
        DateTime submittedAt) : base()
    {
        CVApplicationId = cvApplicationId;
        UserId = userId;
        CompanyId = companyId;
        DepartmentIds = departmentIds;
        ApplicantName = applicantName;
        SubmittedAt = submittedAt;
    }

    public string CVApplicationId { get; }
    public string UserId { get; }
    public string CompanyId { get; }
    public List<string> DepartmentIds { get; }
    public string ApplicantName { get; }
    public DateTime SubmittedAt { get; }
}

/// <summary>
/// CV görüntülendi event'i
/// </summary>
public class CVApplicationViewedEvent : DomainEventBase
{
    public CVApplicationViewedEvent(
        string cvApplicationId,
        string companyId,
        string viewedBy,
        DateTime viewedAt) : base()
    {
        CVApplicationId = cvApplicationId;
        CompanyId = companyId;
        ViewedBy = viewedBy;
        ViewedAt = viewedAt;
    }

    public string CVApplicationId { get; }
    public string CompanyId { get; }
    public string ViewedBy { get; }
    public DateTime ViewedAt { get; }
}

/// <summary>
/// CV indirildi event'i
/// </summary>
public class CVApplicationDownloadedEvent : DomainEventBase
{
    public CVApplicationDownloadedEvent(
        string cvApplicationId,
        string companyId,
        string downloadedBy,
        DateTime downloadedAt,
        DateTime feedbackDeadline) : base()
    {
        CVApplicationId = cvApplicationId;
        CompanyId = companyId;
        DownloadedBy = downloadedBy;
        DownloadedAt = downloadedAt;
        FeedbackDeadline = feedbackDeadline;
    }

    public string CVApplicationId { get; }
    public string CompanyId { get; }
    public string DownloadedBy { get; }
    public DateTime DownloadedAt { get; }
    public DateTime FeedbackDeadline { get; }
}

/// <summary>
/// CV başvurusuna yanıt verildi event'i
/// </summary>
public class CVApplicationRespondedEvent : DomainEventBase
{
    public CVApplicationRespondedEvent(
        string cvApplicationId,
        string companyId,
        string userId,
        CVApplicationStatus status,
        string responseMessage,
        DateTime respondedAt) : base()
    {
        CVApplicationId = cvApplicationId;
        CompanyId = companyId;
        UserId = userId;
        Status = status;
        ResponseMessage = responseMessage;
        RespondedAt = respondedAt;
    }

    public string CVApplicationId { get; }
    public string CompanyId { get; }
    public string UserId { get; }
    public CVApplicationStatus Status { get; }
    public string ResponseMessage { get; }
    public DateTime RespondedAt { get; }
}

/// <summary>
/// CV başvurusu silindi event'i
/// </summary>
public class CVApplicationDeletedEvent : DomainEventBase
{
    public CVApplicationDeletedEvent(
        string cvApplicationId,
        string companyId,
        string userId,
        string reason,
        DateTime deletedAt) : base()
    {
        CVApplicationId = cvApplicationId;
        CompanyId = companyId;
        UserId = userId;
        Reason = reason;
        DeletedAt = deletedAt;
    }

    public string CVApplicationId { get; }
    public string CompanyId { get; }
    public string UserId { get; }
    public string Reason { get; }
    public DateTime DeletedAt { get; }
}

/// <summary>
/// CV geri bildirim süresi doldu event'i
/// </summary>
public class CVApplicationFeedbackOverdueEvent : DomainEventBase
{
    public CVApplicationFeedbackOverdueEvent(
        string cvApplicationId,
        string companyId,
        string userId,
        DateTime feedbackDeadline,
        DateTime overdueAt) : base()
    {
        CVApplicationId = cvApplicationId;
        CompanyId = companyId;
        UserId = userId;
        FeedbackDeadline = feedbackDeadline;
        OverdueAt = overdueAt;
    }

    public string CVApplicationId { get; }
    public string CompanyId { get; }
    public string UserId { get; }
    public DateTime FeedbackDeadline { get; }
    public DateTime OverdueAt { get; }
}