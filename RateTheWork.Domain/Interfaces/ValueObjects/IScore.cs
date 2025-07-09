namespace RateTheWork.Domain.Interfaces.ValueObjects;

/// <summary>
/// Puan value object interface'i
/// </summary>
public interface IScore : IValueObject
{
    decimal Value { get; }
    decimal MinValue { get; }
    decimal MaxValue { get; }
    string Category { get; }
    bool IsValid { get; }
    string GetStarRating();
}
