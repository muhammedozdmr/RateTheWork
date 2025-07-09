namespace RateTheWork.Domain.Interfaces.ValueObjects;

/// <summary>
/// Tarih aralığı value object interface'i
/// </summary>
public interface IDateRange : IValueObject
{
    DateTime StartDate { get; }
    DateTime EndDate { get; }
    TimeSpan Duration { get; }
    bool IsActive { get; }
    bool Contains(DateTime date);
}
