namespace RateTheWork.Domain.Exceptions;

/// <summary>
/// Geçersiz domain durumu exception'ı
/// </summary>
public class InvalidDomainStateException : DomainException
{
    public string EntityName { get; }
    public string CurrentState { get; }
    public string AttemptedAction { get; }

    public InvalidDomainStateException(string entityName, string currentState, string attemptedAction)
        : base($"Cannot perform '{attemptedAction}' on {entityName} in state '{currentState}'.")
    {
        EntityName = entityName;
        CurrentState = currentState;
        AttemptedAction = attemptedAction;
    }
}
