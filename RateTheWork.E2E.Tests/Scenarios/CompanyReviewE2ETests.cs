using FluentAssertions;
using Microsoft.Playwright;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Review;
using RateTheWork.E2E.Tests.Common;

namespace RateTheWork.E2E.Tests.Scenarios;

/// <summary>
/// End-to-end tests for company review functionality.
/// Tests the complete flow of viewing companies, writing reviews, and browsing reviews.
/// </summary>
public class CompanyReviewE2ETests : E2ETestBase
{
    [Fact]
    public async Task WriteCompanyReview_Should_SubmitAndDisplay()
    {
        // Arrange
        var (user, company) = await SeedUserAndCompanyAsync();
        await LoginAsUserAsync(user.Email, "Test123!@#");

        // Act - Navigate to company page
        await Page.GotoAsync($"/companies/{company.Id}");

        // Click write review button
        await Page.ClickAsync("text=Write a Review");

        // Fill review form
        await Page.SelectOptionAsync("select[name='reviewType']", "WorkEnvironment");
        
        // Set ratings
        await Page.ClickAsync(".rating-stars[data-rating='4']");
        
        await Page.FillAsync("textarea[name='reviewText']", 
            "Great work environment with supportive colleagues. The office is modern and well-equipped. " +
            "Management is approachable and values employee feedback.");

        // Add pros and cons
        await Page.FillAsync("input[name='pros']", "Flexible hours, Good benefits, Learning opportunities");
        await Page.FillAsync("input[name='cons']", "Limited parking, Could improve communication");

        // Submit review
        await Page.ClickAsync("button[type='submit']");

        // Wait for success
        var successMessage = await Page.WaitForSelectorAsync("text=Review submitted successfully");
        successMessage.Should().NotBeNull();

        // Verify review appears on company page
        await Page.GotoAsync($"/companies/{company.Id}");
        
        var reviewText = await Page.TextContentAsync(".review-item:first-child .review-text");
        reviewText.Should().Contain("Great work environment");

        await TakeScreenshotAsync("company_review_submitted");
    }

    [Fact]
    public async Task BrowseCompanyReviews_Should_ShowFilteredResults()
    {
        // Arrange
        var company = await SeedCompanyWithReviewsAsync();

        // Act - Navigate to company reviews
        await Page.GotoAsync($"/companies/{company.Id}/reviews");

        // Assert - Check review statistics
        var overallRating = await Page.TextContentAsync(".overall-rating-value");
        overallRating.Should().NotBeNullOrEmpty();

        var totalReviews = await Page.TextContentAsync(".total-reviews");
        totalReviews.Should().Contain("reviews");

        // Apply filter
        await Page.SelectOptionAsync("select[name='reviewType']", "WorkEnvironment");
        await Page.WaitForResponseAsync(resp => resp.Url.Contains("/api/reviews"));

        // Check filtered results
        var reviewItems = await Page.QuerySelectorAllAsync(".review-item");
        reviewItems.Count.Should().BeGreaterThan(0);

        // Check rating distribution
        var ratingBars = await Page.QuerySelectorAllAsync(".rating-distribution-bar");
        ratingBars.Count.Should().Be(5); // 5 stars

        await TakeScreenshotAsync("company_reviews_page");
    }

    [Fact]
    public async Task CompanyComparison_Should_ShowSideBySide()
    {
        // Arrange
        var (company1, company2) = await SeedMultipleCompaniesAsync();

        // Act - Navigate to comparison
        await Page.GotoAsync($"/companies/compare?ids={company1.Id},{company2.Id}");

        // Assert - Check both companies are displayed
        var companyNames = await Page.QuerySelectorAllAsync(".comparison-company-name");
        companyNames.Count.Should().Be(2);

        var company1Name = await Page.TextContentAsync(".comparison-column:first-child .company-name");
        var company2Name = await Page.TextContentAsync(".comparison-column:last-child .company-name");

        company1Name.Should().Be(company1.Name);
        company2Name.Should().Be(company2.Name);

        // Check comparison metrics
        var metricRows = await Page.QuerySelectorAllAsync(".comparison-metric-row");
        metricRows.Count.Should().BeGreaterThan(0);

        await TakeScreenshotAsync("company_comparison");
    }

    [Fact]
    public async Task AnonymousReview_Should_HideUserIdentity()
    {
        // Arrange
        var (user, company) = await SeedUserAndCompanyAsync();
        await LoginAsUserAsync(user.Email, "Test123!@#");

        // Act - Write anonymous review
        await Page.GotoAsync($"/companies/{company.Id}");
        await Page.ClickAsync("text=Write a Review");

        // Check anonymous option
        await Page.CheckAsync("input[name='postAnonymously']");

        await Page.SelectOptionAsync("select[name='reviewType']", "Management");
        await Page.ClickAsync(".rating-stars[data-rating='3']");
        await Page.FillAsync("textarea[name='reviewText']", "Management could be more transparent about company decisions.");

        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForSelectorAsync("text=Review submitted successfully");

        // Navigate back to reviews
        await Page.GotoAsync($"/companies/{company.Id}/reviews");

        // Assert - Review should show as anonymous
        var reviewerName = await Page.TextContentAsync(".review-item:first-child .reviewer-name");
        reviewerName.Should().Contain("Anonymous");
        reviewerName.Should().NotContain(user.FirstName);

        await TakeScreenshotAsync("anonymous_review");
    }

    [Fact]
    public async Task ReviewHelpfulness_Should_UpdateVoteCounts()
    {
        // Arrange
        var company = await SeedCompanyWithReviewsAsync();
        var (user, _) = await SeedUserAndCompanyAsync();
        await LoginAsUserAsync(user.Email, "Test123!@#");

        // Act - Navigate to reviews
        await Page.GotoAsync($"/companies/{company.Id}/reviews");

        // Find helpful button on first review
        var helpfulButton = await Page.WaitForSelectorAsync(".review-item:first-child button[aria-label='Mark as helpful']");
        
        // Get initial count
        var initialCount = await Page.TextContentAsync(".review-item:first-child .helpful-count");

        // Click helpful
        await helpfulButton.ClickAsync();
        
        // Wait for update
        await Page.WaitForResponseAsync(resp => resp.Url.Contains("/api/reviews") && resp.Url.Contains("helpful"));

        // Assert - Count should increase
        var updatedCount = await Page.TextContentAsync(".review-item:first-child .helpful-count");
        var initial = int.Parse(initialCount.Replace(" helpful", ""));
        var updated = int.Parse(updatedCount.Replace(" helpful", ""));
        
        updated.Should().Be(initial + 1);

        await TakeScreenshotAsync("review_helpfulness_voted");
    }

    private async Task<(User user, Company company)> SeedUserAndCompanyAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var user = User.CreateForTesting($"reviewer{Guid.NewGuid()}@example.com", "reviewer123");
            user.SetPassword("Test123!@#");

            var company = Company.Create(
                "Review Test Company",
                "A company for testing reviews",
                "https://reviewtestcompany.com",
                250,
                2015,
                "Technology",
                "Istanbul"
            );

            db.Users.Add(user);
            db.Companies.Add(company);
            await db.SaveChangesAsync();

            return (user, company);
        });
    }

    private async Task<Company> SeedCompanyWithReviewsAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var company = Company.Create(
                "Well Reviewed Company",
                "A company with multiple reviews",
                "https://wellreviewed.com",
                500,
                2010,
                "Technology",
                "Istanbul"
            );

            db.Companies.Add(company);
            await db.SaveChangesAsync();

            // Create multiple users and reviews
            var reviewData = new[]
            {
                (CommentType.WorkEnvironment, 4.5m, "Excellent work environment, very collaborative"),
                (CommentType.Management, 4.0m, "Good management, supportive of employees"),
                (CommentType.CareerGrowth, 3.5m, "Decent growth opportunities but could be better"),
                (CommentType.Benefits, 4.5m, "Great benefits package including health and dental"),
                (CommentType.WorkLifeBalance, 5.0m, "Perfect work-life balance, flexible hours")
            };

            foreach (var (type, rating, text) in reviewData)
            {
                var user = User.CreateForTesting($"user{Guid.NewGuid()}@example.com", $"user{Guid.NewGuid()}");
                db.Users.Add(user);
                
                var review = Review.Create(company.Id, user.Id, type, rating, text);
                db.Reviews.Add(review);
            }

            await db.SaveChangesAsync();
            return company;
        });
    }

    private async Task<(Company company1, Company company2)> SeedMultipleCompaniesAsync()
    {
        return await ExecuteDbContextAsync(async db =>
        {
            var company1 = Company.Create(
                "Tech Giant A",
                "Large technology company",
                "https://techgianta.com",
                5000,
                2005,
                "Technology",
                "Istanbul"
            );

            var company2 = Company.Create(
                "Tech Giant B",
                "Another large tech company",
                "https://techgiantb.com",
                3000,
                2008,
                "Technology",
                "Ankara"
            );

            db.Companies.AddRange(company1, company2);

            // Add reviews for both companies
            for (int i = 0; i < 3; i++)
            {
                var user1 = User.CreateForTesting($"reviewer1_{i}@example.com", $"reviewer1_{i}");
                var user2 = User.CreateForTesting($"reviewer2_{i}@example.com", $"reviewer2_{i}");
                
                db.Users.AddRange(user1, user2);

                var review1 = Review.Create(company1.Id, user1.Id, CommentType.Overall, 4.0m + (i * 0.2m), "Good company");
                var review2 = Review.Create(company2.Id, user2.Id, CommentType.Overall, 3.5m + (i * 0.3m), "Decent company");

                db.Reviews.AddRange(review1, review2);
            }

            await db.SaveChangesAsync();
            return (company1, company2);
        });
    }
}