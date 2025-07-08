namespace RateTheWork.Domain.Exceptions.AggregateException;

/// <summary>
/// Domain servis operasyon hatası exception'ı
/// </summary>
public class DomainServiceOperationException : DomainException
{
    public string ServiceName { get; }
    public string OperationName { get; }
    public Dictionary<string, object> OperationContext { get; }

    public DomainServiceOperationException(string serviceName, string operationName, string reason, Dictionary<string, object> context = null)
        : base($"Domain service operation failed. Service: {serviceName}, Operation: {operationName}, Reason: {reason}")
    {
        ServiceName = serviceName;
        OperationName = operationName;
        OperationContext = context ?? new Dictionary<string, object>();
    }
}
