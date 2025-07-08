namespace RateTheWork.Domain.Exceptions.AggregateException;

/// <summary>
/// Concurrency conflict exception'ı
/// </summary>
public class ConcurrencyConflictException : DomainException
{
    public string EntityType { get; }
    public Guid EntityId { get; }
    public string ExpectedVersion { get; }
    public string ActualVersion { get; }

    public ConcurrencyConflictException(string entityType, Guid entityId, string expectedVersion, string actualVersion)
        : base($"Concurrency conflict detected for {entityType} with ID '{entityId}'. Expected version: {expectedVersion}, Actual version: {actualVersion}")
    {
        EntityType = entityType;
        EntityId = entityId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
}
