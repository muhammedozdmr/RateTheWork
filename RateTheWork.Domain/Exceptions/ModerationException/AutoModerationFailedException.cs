namespace RateTheWork.Domain.Exceptions.ModerationException;

/// <summary>
/// Otomatik moderasyon başarısız exception'ı
/// </summary>
public class AutoModerationFailedException : DomainException
{
    public string ServiceName { get; }
    public string FailureReason { get; }
    public bool RequiresManualReview { get; }

    public AutoModerationFailedException(string serviceName, string failureReason, bool requiresManualReview)
        : base($"Auto-moderation service '{serviceName}' failed. Reason: {failureReason}")
    {
        ServiceName = serviceName;
        FailureReason = failureReason;
        RequiresManualReview = requiresManualReview;
    }
}
