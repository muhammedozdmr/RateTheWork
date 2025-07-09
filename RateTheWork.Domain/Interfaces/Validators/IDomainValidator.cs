namespace RateTheWork.Domain.Interfaces.Validators;

/// <summary>
/// Domain validator interface'i
/// </summary>
public interface IDomainValidator<T>
{
    /// <summary>
    /// Validasyon kurallarını çalıştırır
    /// </summary>
    ValidationResult Validate(T entity);
    
    /// <summary>
    /// Asenkron validasyon
    /// </summary>
    Task<ValidationResult> ValidateAsync(T entity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Validasyon sonucu
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    
    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(params ValidationError[] errors) => new() 
    { 
        IsValid = false, 
        Errors = errors.ToList() 
    };
}

/// <summary>
/// Validasyon hatası
/// </summary>
public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public object? AttemptedValue { get; set; }
}
