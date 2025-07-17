namespace RateTheWork.Domain.Exceptions.StateException;

/// <summary>
/// Geçersiz state geçişi exception'ı
/// </summary>
public class InvalidStateTransitionException : DomainException
{
    public InvalidStateTransitionException
    (
        string entityType
        , string entityId
        , string currentState
        , string targetState
        , IEnumerable<string> allowedStates
    )
        : base(
            $"Invalid state transition for {entityType} '{entityId}' from '{currentState}' to '{targetState}'. Allowed transitions: {string.Join(", ", allowedStates)}")
    {
        EntityType = entityType;
        EntityId = entityId;
        CurrentState = currentState;
        TargetState = targetState;
        AllowedStates = allowedStates.ToList().AsReadOnly();
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
    /// Hedef state
    /// </summary>
    public string TargetState { get; }

    /// <summary>
    /// İzin verilen state'ler
    /// </summary>
    public IReadOnlyCollection<string> AllowedStates { get; }
}
