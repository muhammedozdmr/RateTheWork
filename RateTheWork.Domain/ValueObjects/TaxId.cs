using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.ValueObjects;

namespace RateTheWork.Domain.ValueObjects;


/// <summary>
/// Vergi numarası value object
/// </summary>
public record TaxId : ITaxId
{
    public string Value { get; }

    private TaxId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));
        
        if (value.Length != 10 || !value.All(char.IsDigit))
            throw new BusinessRuleException("Vergi numarası 10 haneli olmalıdır.");
        
        if (!IsValidTaxId(value))
            throw new BusinessRuleException("Geçersiz vergi numarası.");

        Value = value;
    }

    public static TaxId Create(string value) => new(value);

    private static bool IsValidTaxId(string taxId)
    {
        // Türkiye vergi numarası algoritması
        var digits = taxId.Select(c => int.Parse(c.ToString())).ToArray();
        var sum = 0;
        
        for (int i = 0; i < 9; i++)
        {
            sum += digits[i] * (10 - i);
        }
        
        var remainder = sum % 11;
        var checkDigit = remainder == 0 ? 0 : 11 - remainder;
        
        return checkDigit == digits[9];
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

