namespace RateTheWork.Domain.Exceptions.JobApplicationException;

/// <summary>
/// İş başvurusu işlemleri için exception
/// </summary>
public class JobApplicationException : DomainException
{
    public JobApplicationException(string message)
        : base(message)
    {
        Severity = ExceptionSeverity.Medium;
    }

    public JobApplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Severity = ExceptionSeverity.Medium;
    }

    public static JobApplicationException AlreadyApplied(string userId, string jobPostingId)
    {
        return new JobApplicationException($"User {userId} has already applied to job posting {jobPostingId}")
            .WithContext("UserId", userId)
            .WithContext("JobPostingId", jobPostingId)
            .WithUserMessageKey("job_application.already_applied") as JobApplicationException;
    }

    public static JobApplicationException InvalidStatus(string applicationId, string currentStatus, string targetStatus)
    {
        return new JobApplicationException(
                $"Cannot transition application {applicationId} from {currentStatus} to {targetStatus}")
            .WithContext("ApplicationId", applicationId)
            .WithContext("CurrentStatus", currentStatus)
            .WithContext("TargetStatus", targetStatus) as JobApplicationException;
    }

    public static JobApplicationException AlreadyProcessed(string applicationId, string status)
    {
        return new JobApplicationException(
                $"Application {applicationId} has already been processed with status: {status}")
            .WithContext("ApplicationId", applicationId)
            .WithContext("Status", status) as JobApplicationException;
    }

    public static JobApplicationException InterviewDateInPast(string applicationId, DateTime interviewDate)
    {
        return new JobApplicationException(
                $"Cannot schedule interview for application {applicationId} in the past: {interviewDate}")
            .WithContext("ApplicationId", applicationId)
            .WithContext("InterviewDate", interviewDate) as JobApplicationException;
    }

    public static JobApplicationException InvalidSalaryOffer(string applicationId, decimal offeredSalary)
    {
        return new JobApplicationException($"Invalid salary offer {offeredSalary} for application {applicationId}")
            .WithContext("ApplicationId", applicationId)
            .WithContext("OfferedSalary", offeredSalary) as JobApplicationException;
    }

    public static JobApplicationException JobPostingClosed(string applicationId, string jobPostingId)
    {
        return new JobApplicationException(
                $"Cannot process application {applicationId} because job posting {jobPostingId} is closed")
            .WithContext("ApplicationId", applicationId)
            .WithContext("JobPostingId", jobPostingId)
            .WithUserMessageKey("job_application.posting_closed") as JobApplicationException;
    }

    public static JobApplicationException WithdrawnApplication(string applicationId)
    {
        return new JobApplicationException($"Application {applicationId} has been withdrawn and cannot be processed")
            .WithContext("ApplicationId", applicationId)
            .WithUserMessageKey("job_application.withdrawn") as JobApplicationException;
    }
}
