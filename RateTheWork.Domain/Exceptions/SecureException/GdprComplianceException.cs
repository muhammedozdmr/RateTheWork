namespace RateTheWork.Domain.Exceptions.SecureException;

/// <summary>
/// GDPR uyumluluk ihlali exception'ı
/// </summary>
public class GdprComplianceException : DomainException
{
    public string ViolationType { get; }
    public string AffectedData { get; }
    public string RequiredAction { get; }

    public GdprComplianceException(string violationType, string affectedData, string requiredAction)
        : base($"GDPR compliance violation: {violationType}. Affected data: {affectedData}. Required action: {requiredAction}")
    {
        ViolationType = violationType;
        AffectedData = affectedData;
        RequiredAction = requiredAction;
    }
}
