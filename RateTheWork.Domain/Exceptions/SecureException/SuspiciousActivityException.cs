namespace RateTheWork.Domain.Exceptions.SecureException;

/// <summary>
/// Şüpheli aktivite tespit edildi exception'ı
/// </summary>
public class SuspiciousActivityException : DomainException
{
    public string ActivityType { get; }
    public Guid? UserId { get; }
    public string IpAddress { get; }
    public decimal RiskScore { get; }

    public SuspiciousActivityException(string activityType, Guid? userId, string ipAddress, decimal riskScore)
        : base($"Suspicious activity detected: {activityType}. Risk score: {riskScore}/100")
    {
        ActivityType = activityType;
        UserId = userId;
        IpAddress = ipAddress;
        RiskScore = riskScore;
    }
}
