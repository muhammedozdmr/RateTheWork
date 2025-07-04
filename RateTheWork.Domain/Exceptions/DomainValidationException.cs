namespace RateTheWork.Domain.Exceptions;

/// <summary>
/// Domain validasyon exception'ı
/// </summary>
public class DomainValidationException : DomainException
{
    public Dictionary<string, List<string>> Errors { get; }

    public DomainValidationException(string fieldName, string error)
        : base($"Validation failed for {fieldName}: {error}")
    {
        Errors = new Dictionary<string, List<string>>
        {
            { fieldName, new List<string> { error } }
        };
    }

    public DomainValidationException(Dictionary<string, List<string>> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
