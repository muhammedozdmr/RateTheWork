namespace RateTheWork.Domain.Exceptions.ValidationException;

/// <summary>
/// Domain validasyon exception'ı
/// </summary>
public class DomainValidationException : DomainException
{
    public DomainValidationException(string message, IEnumerable<DomainValidationError> errors)
        : base(message)
    {
        Errors = errors.ToList().AsReadOnly();
    }

    public DomainValidationException(string message, DomainValidationError error)
        : base(message)
    {
        Errors = new List<DomainValidationError> { error }.AsReadOnly();
    }

    public DomainValidationException(string entityType, string entityId, IEnumerable<DomainValidationError> errors)
        : base($"Validation failed for {entityType} with id '{entityId}'")
    {
        EntityType = entityType;
        EntityId = entityId;
        Errors = errors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Validasyon hataları
    /// </summary>
    public IReadOnlyCollection<DomainValidationError> Errors { get; }

    /// <summary>
    /// Validate edilen entity tipi
    /// </summary>
    public string? EntityType { get; }

    /// <summary>
    /// Validate edilen entity ID'si
    /// </summary>
    public string? EntityId { get; }

    /// <summary>
    /// Tek bir property için validasyon hatası
    /// </summary>
    public static DomainValidationException ForProperty
        (string propertyName, string message, object? attemptedValue = null)
    {
        var error = new DomainValidationError("VALIDATION_ERROR", propertyName, message, attemptedValue);
        return new DomainValidationException($"Validation failed for property '{propertyName}'", error);
    }

    /// <summary>
    /// Birden fazla property için validasyon hatası
    /// </summary>
    public static DomainValidationException ForMultipleProperties(params (string propertyName, string message)[] errors)
    {
        var validationErrors = errors.Select(e =>
            new DomainValidationError("VALIDATION_ERROR", e.propertyName, e.message)
        ).ToList();

        return new DomainValidationException("Multiple validation errors occurred", validationErrors);
    }
}
