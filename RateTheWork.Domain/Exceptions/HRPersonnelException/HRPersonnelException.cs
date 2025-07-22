namespace RateTheWork.Domain.Exceptions.HRPersonnelException;

/// <summary>
/// İK personeli işlemleri için exception
/// </summary>
public class HRPersonnelException : DomainException
{
    public HRPersonnelException(string message)
        : base(message)
    {
        Severity = ExceptionSeverity.Medium;
    }

    public HRPersonnelException(string message, Exception innerException)
        : base(message, innerException)
    {
        Severity = ExceptionSeverity.Medium;
    }

    public static HRPersonnelException NotActive(string personnelId)
    {
        return new HRPersonnelException($"HR Personnel {personnelId} is not active")
            .WithContext("PersonnelId", personnelId)
            .WithUserMessageKey("hr_personnel.not_active") as HRPersonnelException;
    }

    public static HRPersonnelException NotVerified(string personnelId)
    {
        return new HRPersonnelException($"HR Personnel {personnelId} is not verified")
            .WithContext("PersonnelId", personnelId)
            .WithUserMessageKey("hr_personnel.not_verified") as HRPersonnelException;
    }

    public static HRPersonnelException InvalidTrustScore(string personnelId, decimal score)
    {
        return new HRPersonnelException($"Invalid trust score {score} for HR Personnel {personnelId}")
            .WithContext("PersonnelId", personnelId)
            .WithContext("Score", score) as HRPersonnelException;
    }

    public static HRPersonnelException LowTrustScore(string personnelId, decimal score, decimal requiredScore)
    {
        return new HRPersonnelException(
                $"HR Personnel {personnelId} has low trust score {score}. Required: {requiredScore}")
            .WithContext("PersonnelId", personnelId)
            .WithContext("CurrentScore", score)
            .WithContext("RequiredScore", requiredScore)
            .WithSeverity(ExceptionSeverity.High) as HRPersonnelException;
    }

    public static HRPersonnelException AlreadyVerified(string personnelId)
    {
        return new HRPersonnelException($"HR Personnel {personnelId} is already verified")
            .WithContext("PersonnelId", personnelId) as HRPersonnelException;
    }

    public static HRPersonnelException InvalidTransition(string personnelId, string currentStatus, string targetStatus)
    {
        return new HRPersonnelException(
                $"Cannot transition HR Personnel {personnelId} from {currentStatus} to {targetStatus}")
            .WithContext("PersonnelId", personnelId)
            .WithContext("CurrentStatus", currentStatus)
            .WithContext("TargetStatus", targetStatus) as HRPersonnelException;
    }

    public static HRPersonnelException CompanyMismatch
        (string personnelId, string expectedCompanyId, string actualCompanyId)
    {
        return new HRPersonnelException(
                $"HR Personnel {personnelId} belongs to company {actualCompanyId}, not {expectedCompanyId}")
            .WithContext("PersonnelId", personnelId)
            .WithContext("ExpectedCompanyId", expectedCompanyId)
            .WithContext("ActualCompanyId", actualCompanyId)
            .WithSeverity(ExceptionSeverity.High) as HRPersonnelException;
    }
}
