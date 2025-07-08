namespace RateTheWork.Domain.Exceptions.AggregateException;

/// <summary>
/// External servis entegrasyon hatası exception'ı
/// </summary>
public class ExternalServiceIntegrationException : DomainException
{
    public string ServiceName { get; }
    public string Operation { get; }
    public int? HttpStatusCode { get; }
    public string ErrorCode { get; }

    public ExternalServiceIntegrationException(string serviceName, string operation, int? httpStatusCode = null, string errorCode = null)
        : base($"External service integration failed. Service: {serviceName}, Operation: {operation}, Status: {httpStatusCode?.ToString() ?? "Unknown"}")
    {
        ServiceName = serviceName;
        Operation = operation;
        HttpStatusCode = httpStatusCode;
        ErrorCode = errorCode;
    }
}
