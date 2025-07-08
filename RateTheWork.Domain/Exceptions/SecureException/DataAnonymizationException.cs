namespace RateTheWork.Domain.Exceptions.SecureException;

/// <summary>
/// Veri anonimleştirme hatası exception'ı
/// </summary>
public class DataAnonymizationException : DomainException
{
    public string DataType { get; }
    public string FailureReason { get; }

    public DataAnonymizationException(string dataType, string failureReason)
        : base($"Failed to anonymize {dataType}. Reason: {failureReason}")
    {
        DataType = dataType;
        FailureReason = failureReason;
    }
}
