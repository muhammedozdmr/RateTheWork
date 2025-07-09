namespace RateTheWork.Domain.Interfaces.ValueObjects;

/// <summary>
/// Email value object interface'i
/// </summary>
public interface IEmail : IValueObject
{
    string Value { get; }
    string Domain { get; }
    string LocalPart { get; }
    bool IsBusinessEmail { get; }
}
