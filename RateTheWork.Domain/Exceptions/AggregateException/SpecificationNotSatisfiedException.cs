namespace RateTheWork.Domain.Exceptions.AggregateException;

/// <summary>
/// Specification karşılanmadı exception'ı
/// </summary>
public class SpecificationNotSatisfiedException : DomainException
{
    public string SpecificationName { get; }
    public string EntityType { get; }
    public Dictionary<string, string> FailedCriteria { get; }

    public SpecificationNotSatisfiedException(string specificationName, string entityType, Dictionary<string, string> failedCriteria)
        : base($"Specification '{specificationName}' not satisfied for {entityType}. Failed criteria: {string.Join(", ", failedCriteria.Select(kv => $"{kv.Key}: {kv.Value}"))}")
    {
        SpecificationName = specificationName;
        EntityType = entityType;
        FailedCriteria = failedCriteria;
    }
}
