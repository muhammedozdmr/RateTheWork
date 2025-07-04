namespace RateTheWork.Domain.Exceptions;

/// <summary>
/// Domain işlem zaman aşımı exception'ı
/// </summary>
public class DomainOperationTimeoutException : DomainException
{
    public string Operation { get; }
    public TimeSpan Timeout { get; }

    public DomainOperationTimeoutException(string operation, TimeSpan timeout)
        : base($"Operation '{operation}' timed out after {timeout.TotalSeconds} seconds.")
    {
        Operation = operation;
        Timeout = timeout;
    }
}
