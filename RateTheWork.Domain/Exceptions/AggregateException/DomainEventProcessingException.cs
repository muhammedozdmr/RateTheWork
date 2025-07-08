namespace RateTheWork.Domain.Exceptions.AggregateException;

/// <summary>
/// Domain event işleme hatası exception'ı
/// </summary>
public class DomainEventProcessingException : DomainException
{
    public string EventType { get; }
    public Guid EventId { get; }
    public string HandlerName { get; }

    public DomainEventProcessingException(string eventType, Guid eventId, string handlerName, Exception innerException)
        : base($"Failed to process domain event '{eventType}' (ID: {eventId}) in handler '{handlerName}'.", innerException)
    {
        EventType = eventType;
        EventId = eventId;
        HandlerName = handlerName;
    }
}
