namespace RateTheWork.Domain.ValueObjects.Common;

/// <summary>
/// Value Object base class
/// Domain'de immutable ve değer bazlı eşitlik kontrolü yapan nesneler için
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Eşitlik kontrolü için kullanılacak değerleri döndürür
    /// </summary>
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Value object'in kopyasını oluşturur
    /// </summary>
    protected T Clone<T>() where T : ValueObject
    {
        return (T)MemberwiseClone();
    }
}
