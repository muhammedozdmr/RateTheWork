using FluentAssertions;
using Moq;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Exceptions;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Domain.Interfaces.Services;
using RateTheWork.Domain.Services;
using RateTheWork.Domain.Tests.TestHelpers;

namespace RateTheWork.Domain.Tests.Services;

public class ReviewDomainServiceTests : DomainTestBase
{
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICompanyRepository> _companyRepositoryMock;
    private readonly Mock<IContentModerationService> _contentModerationServiceMock;
    private readonly Mock<IReviewValidationService> _reviewValidationServiceMock;
    private readonly ReviewDomainService _service;

    public ReviewDomainServiceTests()
    {
        _reviewRepositoryMock = CreateMock<IReviewRepository>();
        _userRepositoryMock = CreateMock<IUserRepository>();
        _companyRepositoryMock = CreateMock<ICompanyRepository>();
        _contentModerationServiceMock = CreateMock<IContentModerationService>();
        _reviewValidationServiceMock = CreateMock<IReviewValidationService>();

        _service = new ReviewDomainService(
            _reviewRepositoryMock.Object,
            _userRepositoryMock.Object,
            _companyRepositoryMock.Object,
            _contentModerationServiceMock.Object,
            _reviewValidationServiceMock.Object);
    }

    [Fact]
    public async Task CanCreateReviewAsync_WithValidConditions_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var user = CreateActiveUser(userId);
        var company = CreateActiveCompany(companyId);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId)).ReturnsAsync(company);
        _reviewRepositoryMock.Setup(x => x.GetReviewsByUserAndCompanyAsync(userId, companyId))
            .ReturnsAsync(new List<Review>());

        // Act
        var result = await _service.CanCreateReviewAsync(userId, companyId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanCreateReviewAsync_WithNonExistentUser_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User)null);

        // Act
        var act = async () => await _service.CanCreateReviewAsync(userId, companyId);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task CanCreateReviewAsync_WithBannedUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var user = CreateBannedUser(userId);
        var company = CreateActiveCompany(companyId);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId)).ReturnsAsync(company);

        // Act
        var result = await _service.CanCreateReviewAsync(userId, companyId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanCreateReviewAsync_WithInactiveCompany_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var user = CreateActiveUser(userId);
        var company = CreateInactiveCompany(companyId);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId)).ReturnsAsync(company);

        // Act
        var result = await _service.CanCreateReviewAsync(userId, companyId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanCreateReviewAsync_WithRecentReview_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var user = CreateActiveUser(userId);
        var company = CreateActiveCompany(companyId);
        var recentReview = CreateReview(userId, companyId, DateTime.UtcNow.AddDays(-10));

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId)).ReturnsAsync(company);
        _reviewRepositoryMock.Setup(x => x.GetReviewsByUserAndCompanyAsync(userId, companyId))
            .ReturnsAsync(new List<Review> { recentReview });

        // Act
        var result = await _service.CanCreateReviewAsync(userId, companyId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanCreateReviewAsync_WithOldReview_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var user = CreateActiveUser(userId);
        var company = CreateActiveCompany(companyId);
        var oldReview = CreateReview(userId, companyId, DateTime.UtcNow.AddDays(-100));

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId)).ReturnsAsync(company);
        _reviewRepositoryMock.Setup(x => x.GetReviewsByUserAndCompanyAsync(userId, companyId))
            .ReturnsAsync(new List<Review> { oldReview });

        // Act
        var result = await _service.CanCreateReviewAsync(userId, companyId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateReviewContentAsync_WithValidContent_ShouldReturnSuccess()
    {
        // Arrange
        var title = "Great workplace";
        var content = "I really enjoyed working at this company. The team was supportive and the projects were interesting.";
        var rating = 4.5;

        _contentModerationServiceMock.Setup(x => x.ModerateContentAsync(It.IsAny<string>()))
            .ReturnsAsync(new ContentModerationResult { IsApproved = true });
        _reviewValidationServiceMock.Setup(x => x.ValidateReviewContentAsync(title, content))
            .ReturnsAsync(new ReviewValidationResult { IsValid = true });

        // Act
        var result = await _service.ValidateReviewContentAsync(title, content, rating);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateReviewContentAsync_WithInappropriateContent_ShouldReturnFailure()
    {
        // Arrange
        var title = "Terrible place";
        var content = "This company is [inappropriate content]";
        var rating = 1.0;

        _contentModerationServiceMock.Setup(x => x.ModerateContentAsync(It.IsAny<string>()))
            .ReturnsAsync(new ContentModerationResult 
            { 
                IsApproved = false, 
                Reason = "Inappropriate language detected" 
            });

        // Act
        var result = await _service.ValidateReviewContentAsync(title, content, rating);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Inappropriate language detected");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(5.5)]
    [InlineData(10.0)]
    public async Task ValidateReviewContentAsync_WithInvalidRating_ShouldReturnFailure(double invalidRating)
    {
        // Arrange
        var title = "Review";
        var content = "Content";

        // Act
        var result = await _service.ValidateReviewContentAsync(title, content, invalidRating);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Rating must be between 0.5 and 5.0");
    }

    [Fact]
    public async Task UpdateReviewContentAsync_WithValidReview_ShouldUpdateContent()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var review = CreateReview(userId, Guid.NewGuid(), DateTime.UtcNow.AddDays(-5));
        var newTitle = "Updated Title";
        var newContent = "Updated content with more details";
        var newRating = 4.0;

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId)).ReturnsAsync(review);
        _contentModerationServiceMock.Setup(x => x.ModerateContentAsync(It.IsAny<string>()))
            .ReturnsAsync(new ContentModerationResult { IsApproved = true });
        _reviewValidationServiceMock.Setup(x => x.ValidateReviewContentAsync(newTitle, newContent))
            .ReturnsAsync(new ReviewValidationResult { IsValid = true });

        // Act
        await _service.UpdateReviewContentAsync(reviewId, userId, newTitle, newContent, newRating);

        // Assert
        _reviewRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Review>(r => 
            r.Title == newTitle && 
            r.Content == newContent && 
            r.Rating == newRating)), Times.Once);
    }

    [Fact]
    public async Task UpdateReviewContentAsync_WithNonExistentReview_ShouldThrowException()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId)).ReturnsAsync((Review)null);

        // Act
        var act = async () => await _service.UpdateReviewContentAsync(reviewId, userId, "Title", "Content", 4.0);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage("Review not found");
    }

    [Fact]
    public async Task UpdateReviewContentAsync_WithDifferentUser_ShouldThrowException()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var review = CreateReview(ownerId, Guid.NewGuid(), DateTime.UtcNow.AddDays(-5));

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId)).ReturnsAsync(review);

        // Act
        var act = async () => await _service.UpdateReviewContentAsync(reviewId, otherUserId, "Title", "Content", 4.0);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedDomainActionException>()
            .WithMessage("User is not authorized to update this review");
    }

    [Fact]
    public async Task GetUserReviewCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedCount = 5;

        _reviewRepositoryMock.Setup(x => x.GetUserReviewCountAsync(userId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetUserReviewCountAsync(userId);

        // Assert
        result.Should().Be(expectedCount);
    }

    [Fact]
    public async Task GetCompanyAverageRatingAsync_WithReviews_ShouldReturnAverage()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var expectedAverage = 4.2;

        _reviewRepositoryMock.Setup(x => x.GetCompanyAverageRatingAsync(companyId))
            .ReturnsAsync(expectedAverage);

        // Act
        var result = await _service.GetCompanyAverageRatingAsync(companyId);

        // Assert
        result.Should().Be(expectedAverage);
    }

    [Fact]
    public async Task GetCompanyAverageRatingAsync_WithNoReviews_ShouldReturnZero()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        _reviewRepositoryMock.Setup(x => x.GetCompanyAverageRatingAsync(companyId))
            .ReturnsAsync(0.0);

        // Act
        var result = await _service.GetCompanyAverageRatingAsync(companyId);

        // Assert
        result.Should().Be(0.0);
    }

    private User CreateActiveUser(Guid userId)
    {
        var user = new User
        {
            Id = userId,
            IsBanned = false,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };
        
        // Use reflection to set private properties
        var isActiveProperty = user.GetType().GetProperty("IsActive");
        isActiveProperty?.SetValue(user, true);

        return user;
    }

    private User CreateBannedUser(Guid userId)
    {
        var user = new User
        {
            Id = userId,
            IsBanned = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };
        
        return user;
    }

    private Company CreateActiveCompany(Guid companyId)
    {
        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            IsApproved = true,
            ApprovalStatus = ApprovalStatus.Approved,
            CreatedAt = DateTime.UtcNow.AddYears(-1)
        };

        // Use reflection to set private properties
        var isActiveProperty = company.GetType().GetProperty("IsActive");
        isActiveProperty?.SetValue(company, true);

        return company;
    }

    private Company CreateInactiveCompany(Guid companyId)
    {
        var company = new Company
        {
            Id = companyId,
            Name = "Inactive Company",
            IsApproved = false,
            ApprovalStatus = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddYears(-1)
        };

        // Use reflection to set private properties
        var isActiveProperty = company.GetType().GetProperty("IsActive");
        isActiveProperty?.SetValue(company, false);

        return company;
    }

    private Review CreateReview(Guid userId, Guid companyId, DateTime createdAt)
    {
        var review = new Review
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyId = companyId,
            Title = "Test Review",
            Content = "Test content",
            Rating = 4.0,
            CreatedAt = createdAt,
            IsPublished = true
        };

        return review;
    }
}