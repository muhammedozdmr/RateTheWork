namespace RateTheWork.Domain.Interfaces.ValueObjects;

/// <summary>
/// Value object base interface'i
/// </summary>
public interface IValueObject
{
    /// <summary>
    /// Value object'lerin eşitlik kontrolü için kullanılacak değerler
    /// </summary>
    IEnumerable<object> GetEqualityComponents();
}
