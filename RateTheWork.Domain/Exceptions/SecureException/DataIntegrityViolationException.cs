namespace RateTheWork.Domain.Exceptions.SecureException;

/// <summary>
/// Veri bütünlüğü ihlali exception'ı
/// </summary>
public class DataIntegrityViolationException : DomainException
{
    public string EntityType { get; }
    public string ViolationType { get; }
    public string ExpectedHash { get; }
    public string ActualHash { get; }

    public DataIntegrityViolationException(string entityType, string violationType, string expectedHash, string actualHash)
        : base($"Data integrity violation detected for {entityType}. Type: {violationType}")
    {
        EntityType = entityType;
        ViolationType = violationType;
        ExpectedHash = expectedHash;
        ActualHash = actualHash;
    }
}
