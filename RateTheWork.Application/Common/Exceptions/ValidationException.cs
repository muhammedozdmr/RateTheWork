using FluentValidation.Results;

namespace RateTheWork.Application.Common.Exceptions;

/// <summary>
/// Validation hatalarını temsil eden exception.
/// FluentValidation failure'larını içerir.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Validation hataları dictionary'si
    /// Key: Property adı, Value: Hata mesajları
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Boş validation exception oluşturur
    /// </summary>
    public ValidationException()
        : base("Bir veya daha fazla validation hatası oluştu.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Tek bir hata mesajı ile exception oluşturur
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>
        {
            [""] = new[] { message }
        };
    }

    /// <summary>
    /// FluentValidation failure'larından exception oluşturur
    /// </summary>
    /// <param name="failures">Validation hataları</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}
