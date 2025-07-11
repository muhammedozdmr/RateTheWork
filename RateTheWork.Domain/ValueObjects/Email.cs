using System.Text.RegularExpressions;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.ValueObjects;

namespace RateTheWork.Domain.ValueObjects;

/// <summary>
/// Email value object
/// </summary>
public record Email : IEmail
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }
    public string Domain { get; }
    public string LocalPart { get; }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));

        if (!EmailRegex.IsMatch(value))
            throw new BusinessRuleException("Geçersiz email formatı.");

        Value = value.ToLowerInvariant();
        var parts = Value.Split('@');
        LocalPart = parts[0];
        Domain = parts[1];
    }

    public static Email Create(string value) => new(value);

    public bool IsBusinessEmail => 
        !Domain.Contains("gmail") && 
        !Domain.Contains("hotmail") && 
        !Domain.Contains("yahoo") &&
        !Domain.Contains("outlook");

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
