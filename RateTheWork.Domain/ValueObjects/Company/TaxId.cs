using RateTheWork.Domain.Constants;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.ValueObjects;

namespace RateTheWork.Domain.ValueObjects.Company;

/// <summary>
/// Vergi numarası value object
/// </summary>
public record TaxId : ITaxId
{
    private TaxId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));

        if (value.Length != DomainConstants.Company.TaxIdLength || !value.All(char.IsDigit))
            throw new BusinessRuleException($"Vergi numarası {DomainConstants.Company.TaxIdLength} haneli olmalıdır.");

        if (!IsValidTaxId(value))
            throw new BusinessRuleException("Geçersiz vergi numarası.");

        Value = value;
    }

    public string Value { get; }

    public static TaxId Create(string value) => new(value);

    private static bool IsValidTaxId(string taxId)
    {
        // Türkiye vergi numarası algoritması
        var digits = taxId.Select(c => int.Parse(c.ToString())).ToArray();
        var sum = 0;

        for (int i = 0; i < DomainConstants.Company.TaxIdValidationLength; i++)
        {
            sum += digits[i] * (DomainConstants.Company.TaxIdValidationMultiplier - i);
        }

        var remainder = sum % DomainConstants.Company.TaxIdValidationModulus;
        var checkDigit = remainder == 0 ? 0 : DomainConstants.Company.TaxIdValidationModulus - remainder;

        return checkDigit == digits[DomainConstants.Company.TaxIdValidationLength];
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
