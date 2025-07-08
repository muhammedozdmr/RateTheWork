namespace RateTheWork.Domain.Exceptions.SecureException;

/// <summary>
/// Veri şifreleme hatası exception'ı
/// </summary>
public class DataEncryptionException : DomainException
{
    public string OperationType { get; }
    public string DataType { get; }

    public DataEncryptionException(string operationType, string dataType, Exception innerException)
        : base($"Data encryption operation '{operationType}' failed for data type '{dataType}'.", innerException)
    {
        OperationType = operationType;
        DataType = dataType;
    }

    public DataEncryptionException(string message)
        : base(message)
    {
        OperationType = "Unknown";
        DataType = "Unknown";
    }
}
