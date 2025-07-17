using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.ValueObjects;

namespace RateTheWork.Domain.ValueObjects.Common;

/// <summary>
/// Para birimi value object
/// </summary>
public record Money : IMoney
{
    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new BusinessRuleException("Para miktarı negatif olamaz.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentNullException(nameof(currency));

        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public string FormattedValue => $"{Amount:N2} {Currency}";

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public static Money Create(decimal amount, string currency = "TRY")
        => new(amount, currency);

    public static Money Zero(string currency = "TRY")
        => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new BusinessRuleException("Farklı para birimleri toplanamaz.");

        return new(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new BusinessRuleException("Farklı para birimleri çıkarılamaz.");

        return new(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier)
        => new(Amount * multiplier, Currency);
}
