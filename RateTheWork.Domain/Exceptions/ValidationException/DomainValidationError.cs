namespace RateTheWork.Domain.Exceptions.ValidationException;

/// <summary>
/// Domain validasyon hatası detayı
/// </summary>
public class DomainValidationError
{
    public DomainValidationError
    (
        string code
        , string propertyName
        , string message
        , object? attemptedValue = null
        , ValidationSeverity severity = ValidationSeverity.Error
    )
    {
        Code = code;
        PropertyName = propertyName;
        Message = message;
        AttemptedValue = attemptedValue;
        Severity = severity;
    }

    /// <summary>
    /// Hata kodu
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Property adı
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Hata mesajı
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Geçersiz değer
    /// </summary>
    public object? AttemptedValue { get; }

    /// <summary>
    /// Hata seviyesi
    /// </summary>
    public ValidationSeverity Severity { get; }
}

/// <summary>
/// Validasyon hata seviyesi
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Bilgi
    /// </summary>
    Info

    ,

    /// <summary>
    /// Uyarı
    /// </summary>
    Warning

    ,

    /// <summary>
    /// Hata
    /// </summary>
    Error

    ,

    /// <summary>
    /// Kritik
    /// </summary>
    Critical
}
