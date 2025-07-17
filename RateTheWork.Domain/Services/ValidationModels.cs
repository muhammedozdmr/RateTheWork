namespace RateTheWork.Domain.Services;

/// <summary>
/// Validasyon sonucu modeli
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();

    public static ValidationResult Success() => new() { IsValid = true };

    public static ValidationResult Failure(ValidationError error) => new()
    {
        IsValid = false, Errors = new List<ValidationError> { error }
    };

    public static ValidationResult Failure(List<ValidationError> errors) => new()
    {
        IsValid = false, Errors = errors
    };
}

/// <summary>
/// Validasyon hatası modeli
/// </summary>
public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public object? AttemptedValue { get; set; }
}
