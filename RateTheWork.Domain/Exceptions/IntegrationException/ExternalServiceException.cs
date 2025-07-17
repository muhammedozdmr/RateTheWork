namespace RateTheWork.Domain.Exceptions.IntegrationException;

/// <summary>
/// Dış servis entegrasyon exception'ı
/// </summary>
public class ExternalServiceException : DomainException
{
    public ExternalServiceException
    (
        string serviceName
        , string operation
        , string message
        , int? httpStatusCode = null
        , string? serviceErrorCode = null
        , bool isRetryable = false
        , TimeSpan? retryAfter = null
    )
        : base($"External service '{serviceName}' failed during '{operation}': {message}")
    {
        ServiceName = serviceName;
        Operation = operation;
        HttpStatusCode = httpStatusCode;
        ServiceErrorCode = serviceErrorCode;
        IsRetryable = isRetryable;
        RetryAfter = retryAfter;
        RequestId = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Servis adı
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// İşlem tipi
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// HTTP durum kodu (varsa)
    /// </summary>
    public int? HttpStatusCode { get; }

    /// <summary>
    /// Servis hata kodu
    /// </summary>
    public string? ServiceErrorCode { get; }

    /// <summary>
    /// İstek ID'si (debugging için)
    /// </summary>
    public string? RequestId { get; }

    /// <summary>
    /// Yeniden denenebilir mi?
    /// </summary>
    public bool IsRetryable { get; }

    /// <summary>
    /// Tavsiye edilen bekleme süresi
    /// </summary>
    public TimeSpan? RetryAfter { get; }
}
