namespace RateTheWork.Domain.Exceptions;

/// <summary>
/// Entity bulunamadı exception'ı
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, string entityId)
        : base($"{entityName} with id '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public EntityNotFoundException(string message)
        : base(message)
    {
        EntityName = "Unknown";
        EntityId = "Unknown";
    }

    /// <summary>
    /// Arama kriterleri ile exception oluştur
    /// </summary>
    public EntityNotFoundException(string entityName, Dictionary<string, object?> searchCriteria)
        : base(
            $"{entityName} not found with criteria: {string.Join(", ", searchCriteria.Select(kvp => $"{kvp.Key}='{kvp.Value}'"))}")
    {
        EntityName = entityName;
        EntityId = "N/A";
        SearchCriteria = searchCriteria;
    }

    /// <summary>
    /// Entity adı
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Entity ID'si
    /// </summary>
    public string EntityId { get; }

    /// <summary>
    /// Arama kriterleri
    /// </summary>
    public Dictionary<string, object?>? SearchCriteria { get; }

    /// <summary>
    /// İşlem context'i
    /// </summary>
    public string? OperationContext { get; }

    /// <summary>
    /// Birden fazla entity bulunamadığında
    /// </summary>
    public static EntityNotFoundException ForMultiple(string entityName, IEnumerable<string> entityIds)
    {
        var idList = string.Join(", ", entityIds);
        return new EntityNotFoundException($"Multiple {entityName} entities not found: {idList}");
    }
}
