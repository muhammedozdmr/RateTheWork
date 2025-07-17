namespace RateTheWork.Domain.Exceptions.StateException;

/// <summary>
/// Entity gereken state'de değil exception'ı
/// </summary>
public class EntityNotInRequiredStateException : DomainException
{
    public EntityNotInRequiredStateException
    (
        string entityType
        , string entityId
        , string currentState
        , IEnumerable<string> requiredStates
        , string operation
    )
        : base(
            $"{entityType} '{entityId}' must be in one of the following states to perform '{operation}': {string.Join(", ", requiredStates)}. Current state: '{currentState}'")
    {
        EntityType = entityType;
        EntityId = entityId;
        CurrentState = currentState;
        RequiredStates = requiredStates.ToList().AsReadOnly();
        Operation = operation;
    }

    /// <summary>
    /// Entity tipi
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Entity ID'si
    /// </summary>
    public string EntityId { get; }

    /// <summary>
    /// Mevcut state
    /// </summary>
    public string CurrentState { get; }

    /// <summary>
    /// Gereken state'ler
    /// </summary>
    public IReadOnlyCollection<string> RequiredStates { get; }

    /// <summary>
    /// İşlem adı
    /// </summary>
    public string Operation { get; }
}
