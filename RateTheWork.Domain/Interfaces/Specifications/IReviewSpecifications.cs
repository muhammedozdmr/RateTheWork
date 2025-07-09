using RateTheWork.Domain.Entities;

namespace RateTheWork.Domain.Interfaces.Specifications;

/// <summary>
/// Özel review specification'ları için interface
/// </summary>
public interface IReviewSpecifications
{
    ISpecification<Review> ActiveReviews();
    ISpecification<Review> VerifiedReviews();
    ISpecification<Review> ReviewsByCompany(string companyId);
    ISpecification<Review> ReviewsByUser(string userId);
    ISpecification<Review> ReviewsInModerationQueue();
    ISpecification<Review> ReviewsWithMinimumScore(decimal minScore);
    ISpecification<Review> SpamReviews();
}
