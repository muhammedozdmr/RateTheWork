using RateTheWork.Domain.Constants;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.ValueObjects;

namespace RateTheWork.Domain.ValueObjects.Common;

/// <summary>
/// Puan value object
/// </summary>
public record Rating : IScore
{
    private Rating(decimal value, string category = "Overall")
    {
        if (value < MinValue || value > MaxValue)
            throw new BusinessRuleException($"Puan {MinValue} ile {MaxValue} arasında olmalıdır.");

        if (value % DomainConstants.Review.RatingStep != 0)
            throw new BusinessRuleException($"Puan {DomainConstants.Review.RatingStep}'lik artışlarla verilebilir.");

        Value = value;
        Category = category;
    }

    public decimal Value { get; }
    public decimal MinValue => DomainConstants.Review.MinRating;
    public decimal MaxValue => DomainConstants.Review.MaxRating;
    public string Category { get; }

    public bool IsValid => Value >= MinValue && Value <= MaxValue;

    public string GetStarRating()
    {
        var fullStars = (int)Math.Floor(Value);
        var hasHalfStar = Value % 1 != 0;

        var stars = new string('★', fullStars);
        if (hasHalfStar) stars += "½";

        var emptyStars = new string('☆', 5 - fullStars - (hasHalfStar ? 1 : 0));

        return stars + emptyStars;
    }

    public IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Category;
    }

    public static Rating Create(decimal value, string category = "Overall")
        => new(value, category);
}
