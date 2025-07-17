using RateTheWork.Domain.Constants;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.ValueObjects;

namespace RateTheWork.Domain.ValueObjects.Company;

/// <summary>
/// MERSIS numarası value object
/// </summary>
public record MersisNo : IMersisNo
{
    private MersisNo(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));

        var cleaned = value.Replace("-", "").Replace(" ", "");

        if (cleaned.Length != DomainConstants.Company.MersisNoLength || !cleaned.All(char.IsDigit))
            throw new BusinessRuleException(
                $"MERSIS numarası {DomainConstants.Company.MersisNoLength} haneli olmalıdır.");

        Value = cleaned;
    }

    public string Value { get; }

    public static MersisNo Create(string value) => new(value);

    public string GetFormatted()
        =>
            $"{Value.Substring(0, DomainConstants.Company.MersisSegment1Length)}-{Value.Substring(DomainConstants.Company.MersisSegment1Length, DomainConstants.Company.MersisSegment2Length)}-{Value.Substring(DomainConstants.Company.MersisSegment1Length + DomainConstants.Company.MersisSegment2Length, DomainConstants.Company.MersisSegment3Length)}-{Value.Substring(DomainConstants.Company.MersisSegment1Length + DomainConstants.Company.MersisSegment2Length + DomainConstants.Company.MersisSegment3Length, DomainConstants.Company.MersisSegment4Length)}";

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
