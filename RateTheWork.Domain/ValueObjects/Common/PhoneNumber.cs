using System.Text.RegularExpressions;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.ValueObjects;

namespace RateTheWork.Domain.ValueObjects.Common;

/// <summary>
/// Telefon numarası value object
/// </summary>
public record PhoneNumber : IPhoneNumber
{
    private static readonly Regex CleanRegex = new(@"[^\d+]", RegexOptions.Compiled);

    private PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));

        // Temizle
        var cleaned = CleanRegex.Replace(value, "");

        if (cleaned.Length < 10)
            throw new BusinessRuleException("Telefon numarası en az 10 haneli olmalıdır.");

        Value = cleaned;

        // Türkiye formatı
        if (cleaned.StartsWith("+90"))
        {
            CountryCode = "+90";
            Number = cleaned.Substring(3);
        }
        else if (cleaned.StartsWith("90") && cleaned.Length == 12)
        {
            CountryCode = "+90";
            Number = cleaned.Substring(2);
        }
        else if (cleaned.StartsWith("0"))
        {
            CountryCode = "+90";
            Number = cleaned.Substring(1);
        }
        else
        {
            CountryCode = "+90";
            Number = cleaned;
        }
    }

    public string Value { get; }
    public string CountryCode { get; }
    public string Number { get; }

    public static PhoneNumber Create(string value) => new(value);

    public string GetFormatted() => $"{CountryCode} {Number}";

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Number;
    }
}
