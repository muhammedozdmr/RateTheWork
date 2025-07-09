namespace RateTheWork.Domain.Interfaces.ValueObjects;

/// <summary>
/// Para birimi value object interface'i
/// </summary>
public interface IMoney : IValueObject
{
    decimal Amount { get; }
    string Currency { get; }
    string FormattedValue { get; }
}
