namespace RateTheWork.Domain.Exceptions.AggregateException;

/// <summary>
/// Eventual consistency ihlali exception'ı
/// </summary>
public class EventualConsistencyViolationException : DomainException
{
    public string AggregateType { get; }
    public Guid AggregateId { get; }
    public string ConsistencyRule { get; }
    public TimeSpan MaxWaitTime { get; }

    public EventualConsistencyViolationException(string aggregateType, Guid aggregateId, string consistencyRule, TimeSpan maxWaitTime)
        : base($"Eventual consistency violation for {aggregateType} (ID: {aggregateId}). Rule: {consistencyRule}. Max wait time exceeded: {maxWaitTime.TotalSeconds}s")
    {
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        ConsistencyRule = consistencyRule;
        MaxWaitTime = maxWaitTime;
    }
}
