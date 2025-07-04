namespace RateTheWork.Domain.Exceptions;

/// <summary>
/// Entity bulunamadı exception'ı
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public string EntityId { get; }

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
}
