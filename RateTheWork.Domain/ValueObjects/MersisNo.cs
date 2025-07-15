using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.ValueObjects;

namespace RateTheWork.Domain.ValueObjects;

/// <summary>
/// MERSIS numarası value object
/// </summary>
public record MersisNo : IMersisNo
{
    public string Value { get; }

    private MersisNo(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));
        
        var cleaned = value.Replace("-", "").Replace(" ", "");
        
        if (cleaned.Length != 16 || !cleaned.All(char.IsDigit))
            throw new BusinessRuleException("MERSIS numarası 16 haneli olmalıdır.");

        Value = cleaned;
    }

    public static MersisNo Create(string value) => new(value);

    public string GetFormatted() 
        => $"{Value.Substring(0, 4)}-{Value.Substring(4, 4)}-{Value.Substring(8, 4)}-{Value.Substring(12, 4)}";

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
