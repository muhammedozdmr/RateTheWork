namespace RateTheWork.Domain.Exceptions.OperationException;

/// <summary>
/// Toplu işlem exception'ı
/// </summary>
public class BulkOperationException : DomainException
{
    public BulkOperationException
    (
        string operationType
        , int totalCount
        , int successCount
        , IEnumerable<BulkOperationFailure> failures
    )
        : base(
            $"Bulk operation '{operationType}' partially failed. Total: {totalCount}, Success: {successCount}, Failed: {totalCount - successCount}")
    {
        OperationType = operationType;
        TotalCount = totalCount;
        SuccessCount = successCount;
        FailureCount = totalCount - successCount;
        Failures = failures.ToList().AsReadOnly();
    }

    /// <summary>
    /// İşlem tipi
    /// </summary>
    public string OperationType { get; }

    /// <summary>
    /// Toplam işlem sayısı
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Başarılı işlem sayısı
    /// </summary>
    public int SuccessCount { get; }

    /// <summary>
    /// Başarısız işlem sayısı
    /// </summary>
    public int FailureCount { get; }

    /// <summary>
    /// Başarısız işlemlerin detayları
    /// </summary>
    public IReadOnlyCollection<BulkOperationFailure> Failures { get; }

    /// <summary>
    /// İşlem tamamen başarısız mı?
    /// </summary>
    public bool IsCompleteFailure => SuccessCount == 0;

    /// <summary>
    /// İşlem kısmen başarılı mı?
    /// </summary>
    public bool IsPartialSuccess => SuccessCount > 0 && FailureCount > 0;
}

/// <summary>
/// Toplu işlem hatası detayı
/// </summary>
public class BulkOperationFailure
{
    public BulkOperationFailure
    (
        int index
        , string? entityId
        , string errorMessage
        , string? errorCode = null
        , Exception? innerException = null
    )
    {
        Index = index;
        EntityId = entityId;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        InnerException = innerException;
    }

    /// <summary>
    /// İşlem index'i
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Entity ID'si
    /// </summary>
    public string? EntityId { get; }

    /// <summary>
    /// Hata mesajı
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Hata kodu
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// İç exception
    /// </summary>
    public Exception? InnerException { get; }
}
