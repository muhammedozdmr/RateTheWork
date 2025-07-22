namespace RateTheWork.Domain.Exceptions.JobPostingException;

/// <summary>
/// İş ilanı işlemleri için exception
/// </summary>
public class JobPostingException : DomainException
{
    public JobPostingException(string message)
        : base(message)
    {
        Severity = ExceptionSeverity.Medium;
    }

    public JobPostingException(string message, Exception innerException)
        : base(message, innerException)
    {
        Severity = ExceptionSeverity.Medium;
    }

    public static JobPostingException InvalidJobPosting(string reason)
    {
        return new JobPostingException($"Invalid job posting: {reason}")
            .WithContext("Reason", reason) as JobPostingException;
    }

    public static JobPostingException MissingTransparencyInfo(string field)
    {
        return new JobPostingException($"Missing required transparency information: {field}")
            .WithContext("MissingField", field)
            .WithUserMessageKey("job_posting.missing_transparency") as JobPostingException;
    }

    public static JobPostingException InvalidStatus(string jobPostingId, string currentStatus, string targetStatus)
    {
        return new JobPostingException(
                $"Cannot transition job posting {jobPostingId} from {currentStatus} to {targetStatus}")
            .WithContext("JobPostingId", jobPostingId)
            .WithContext("CurrentStatus", currentStatus)
            .WithContext("TargetStatus", targetStatus) as JobPostingException;
    }

    public static JobPostingException AlreadyExpired(string jobPostingId)
    {
        return new JobPostingException($"Job posting {jobPostingId} has already expired")
            .WithContext("JobPostingId", jobPostingId)
            .WithUserMessageKey("job_posting.expired") as JobPostingException;
    }

    public static JobPostingException TargetReached(string jobPostingId, int targetCount)
    {
        return new JobPostingException(
                $"Job posting {jobPostingId} has reached its target application count of {targetCount}")
            .WithContext("JobPostingId", jobPostingId)
            .WithContext("TargetCount", targetCount)
            .WithUserMessageKey("job_posting.target_reached") as JobPostingException;
    }

    public static JobPostingException SuspiciousActivity(string jobPostingId, string[] reasons)
    {
        return new JobPostingException($"Suspicious activity detected for job posting {jobPostingId}")
            .WithContext("JobPostingId", jobPostingId)
            .WithContext("Reasons", reasons)
            .WithSeverity(ExceptionSeverity.High)
            .WithUserMessageKey("job_posting.suspicious_activity") as JobPostingException;
    }

    public static JobPostingException UnauthorizedAction(string jobPostingId, string personnelId, string action)
    {
        return new JobPostingException(
                $"HR Personnel {personnelId} is not authorized to {action} job posting {jobPostingId}")
            .WithContext("JobPostingId", jobPostingId)
            .WithContext("PersonnelId", personnelId)
            .WithContext("Action", action)
            .WithSeverity(ExceptionSeverity.High) as JobPostingException;
    }
}
