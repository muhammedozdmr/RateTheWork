namespace RateTheWork.Domain.Exceptions.AggregateException;

/// <summary>
/// Value object oluşturma hatası exception'ı
/// </summary>
public class InvalidValueObjectException : DomainException
{
    public string ValueObjectType { get; }
    public string PropertyName { get; }
    public object AttemptedValue { get; }

    public InvalidValueObjectException(string valueObjectType, string propertyName, object attemptedValue, string reason)
        : base($"Cannot create {valueObjectType}. Invalid value for {propertyName}: '{attemptedValue}'. Reason: {reason}")
    {
        ValueObjectType = valueObjectType;
        PropertyName = propertyName;
        AttemptedValue = attemptedValue;
    }
}
