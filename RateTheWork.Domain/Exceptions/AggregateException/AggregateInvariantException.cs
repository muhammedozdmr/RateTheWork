namespace RateTheWork.Domain.Exceptions.AggregateException;

/// <summary>
/// Aggregate invariant ihlali exception'ı
/// </summary>
public class AggregateInvariantException : DomainException
{
    public string AggregateName { get; }
    public string InvariantRule { get; }
    public object? CurrentValue { get; }

    public AggregateInvariantException(string aggregateName, string invariantRule, object? currentValue = null)
        : base($"Aggregate invariant violated in {aggregateName}. Rule: {invariantRule}")
    {
        AggregateName = aggregateName;
        InvariantRule = invariantRule;
        CurrentValue = currentValue;
    }
}
