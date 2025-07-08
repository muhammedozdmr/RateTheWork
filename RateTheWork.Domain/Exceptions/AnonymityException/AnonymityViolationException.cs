namespace RateTheWork.Domain.Exceptions.AnonymityException;

/// <summary>
/// Anonim değerlendirme kuralı ihlali exception'ı
/// </summary>
public class AnonymityViolationException : DomainException
{
    public string ViolationType { get; }
    public string AttemptedAction { get; }

    public AnonymityViolationException(string violationType, string attemptedAction)
        : base($"Anonymity violation: {violationType} during '{attemptedAction}'.")
    {
        ViolationType = violationType;
        AttemptedAction = attemptedAction;
    }

    public AnonymityViolationException(string message)
        : base(message)
    {
        ViolationType = "Unknown";
        AttemptedAction = "Unknown";
    }
}
