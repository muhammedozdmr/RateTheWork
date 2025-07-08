namespace RateTheWork.Domain.Exceptions.ModerationException;

/// <summary>
/// Toplu işlem limiti aşıldı exception'ı
/// </summary>
public class BulkOperationLimitException : DomainException
{
    public string OperationType { get; }
    public int RequestedCount { get; }
    public int MaxAllowedCount { get; }

    public BulkOperationLimitException(string operationType, int requestedCount, int maxAllowedCount)
        : base($"Bulk operation limit exceeded for '{operationType}'. Requested: {requestedCount}, Max allowed: {maxAllowedCount}")
    {
        OperationType = operationType;
        RequestedCount = requestedCount;
        MaxAllowedCount = maxAllowedCount;
    }
}
