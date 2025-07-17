namespace RateTheWork.Domain.Exceptions;

/// <summary>
/// Domain katmanı base exception
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
        OccurredOn = DateTime.UtcNow;
        Context = new Dictionary<string, object?>();
        TraceId = Guid.NewGuid().ToString();
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
        OccurredOn = DateTime.UtcNow;
        Context = new Dictionary<string, object?>();
        TraceId = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Exception oluşturulma zamanı
    /// </summary>
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Exception context bilgileri
    /// </summary>
    public Dictionary<string, object?> Context { get; }

    /// <summary>
    /// Exception'ın trace ID'si (debugging için)
    /// </summary>
    public string TraceId { get; }

    /// <summary>
    /// Exception severity level
    /// </summary>
    public ExceptionSeverity Severity { get; protected set; } = ExceptionSeverity.Medium;

    /// <summary>
    /// User-friendly message key for localization
    /// </summary>
    public string? UserMessageKey { get; protected set; }

    /// <summary>
    /// Set severity level
    /// </summary>
    public DomainException WithSeverity(ExceptionSeverity severity)
    {
        Severity = severity;
        return this;
    }

    /// <summary>
    /// Set user message key for localization
    /// </summary>
    public DomainException WithUserMessageKey(string messageKey)
    {
        UserMessageKey = messageKey;
        return this;
    }

    /// <summary>
    /// Context bilgisi ekle
    /// </summary>
    public DomainException WithContext(string key, object? value)
    {
        Context[key] = value;
        return this;
    }

    /// <summary>
    /// Çoklu context bilgisi ekle
    /// </summary>
    public DomainException WithContext(Dictionary<string, object?> context)
    {
        foreach (var item in context)
        {
            Context[item.Key] = item.Value;
        }

        return this;
    }
}
