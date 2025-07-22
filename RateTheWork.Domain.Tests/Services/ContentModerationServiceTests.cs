using FluentAssertions;
using RateTheWork.Domain.Services;
using RateTheWork.Domain.Tests.TestHelpers;

namespace RateTheWork.Domain.Tests.Services;

public class ContentModerationServiceTests : DomainTestBase
{
    private readonly ContentModerationService _service;

    public ContentModerationServiceTests()
    {
        _service = new ContentModerationService();
    }

    [Theory]
    [InlineData("This is a great company to work for!")]
    [InlineData("The work environment is professional and supportive.")]
    [InlineData("I enjoyed my time working here and learned a lot.")]
    public async Task ModerateContentAsync_WithAppropriateContent_ShouldApprove(string content)
    {
        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.IsApproved.Should().BeTrue();
        result.Reason.Should().BeNullOrEmpty();
        result.Confidence.Should().BeGreaterThan(0);
        result.FlaggedWords.Should().BeEmpty();
    }

    [Theory]
    [InlineData("This company is a f***ing disaster", "fuck")]
    [InlineData("The manager is a complete a**hole", "asshole")]
    [InlineData("S**t working conditions", "shit")]
    public async Task ModerateContentAsync_WithProfanity_ShouldReject(string content, string expectedFlag)
    {
        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Contain("profanity");
        result.FlaggedWords.Should().Contain(expectedFlag);
    }

    [Theory]
    [InlineData("Contact me at example@email.com for more info")]
    [InlineData("Email: test.user@company.com")]
    [InlineData("Send details to admin@ratethework.com")]
    public async Task ModerateContentAsync_WithEmail_ShouldReject(string content)
    {
        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Contain("personal information");
        result.Categories.Should().Contain("PersonalInfo");
    }

    [Theory]
    [InlineData("Call me at 555-1234")]
    [InlineData("Phone: +90 555 123 4567")]
    [InlineData("WhatsApp: 05551234567")]
    public async Task ModerateContentAsync_WithPhoneNumber_ShouldReject(string content)
    {
        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Contain("personal information");
        result.Categories.Should().Contain("PersonalInfo");
    }

    [Theory]
    [InlineData("Visit us at www.example.com")]
    [InlineData("Check out https://malicious-site.com")]
    [InlineData("More info: http://suspicious.website")]
    public async Task ModerateContentAsync_WithUrl_ShouldReject(string content)
    {
        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Contain("URL");
        result.Categories.Should().Contain("Spam");
    }

    [Theory]
    [InlineData("BUY NOW!!! BEST OFFER!!!")]
    [InlineData("CLICK HERE FOR AMAZING DEALS!!!")]
    [InlineData("!!!URGENT!!! LIMITED TIME OFFER!!!")]
    public async Task ModerateContentAsync_WithSpamPatterns_ShouldReject(string content)
    {
        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Contain("spam");
        result.Categories.Should().Contain("Spam");
    }

    [Theory]
    [InlineData("I will kill you")]
    [InlineData("You deserve to die")]
    [InlineData("I hope you get hurt")]
    public async Task ModerateContentAsync_WithThreats_ShouldReject(string content)
    {
        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Contain("threatening");
        result.Categories.Should().Contain("Threat");
    }

    [Theory]
    [InlineData("Women don't belong in tech")]
    [InlineData("Only hire people from our race")]
    [InlineData("Old people can't do this job")]
    public async Task ModerateContentAsync_WithDiscrimination_ShouldReject(string content)
    {
        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Contain("discriminatory");
        result.Categories.Should().Contain("Discrimination");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\n")]
    public async Task ModerateContentAsync_WithEmptyContent_ShouldReject(string content)
    {
        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Contain("empty");
    }

    [Fact]
    public async Task ModerateContentAsync_WithMixedViolations_ShouldRejectWithMultipleCategories()
    {
        // Arrange
        var content = "This f***ing company sucks! Email me at test@email.com for the truth.";

        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.IsApproved.Should().BeFalse();
        result.Categories.Should().Contain("Profanity");
        result.Categories.Should().Contain("PersonalInfo");
        result.FlaggedWords.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ModerateContentAsync_WithLongContent_ShouldProcessCorrectly()
    {
        // Arrange
        var longContent = string.Join(" ", Enumerable.Repeat("This is a normal sentence.", 100));

        // Act
        var result = await _service.ModerateContentAsync(longContent);

        // Assert
        result.Should().NotBeNull();
        result.IsApproved.Should().BeTrue();
        result.ProcessingTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Theory]
    [InlineData("The work-life balance is good", true)]
    [InlineData("Management could be better", true)]
    [InlineData("Salary is below market average", true)]
    [InlineData("No career growth opportunities", true)]
    public async Task ModerateContentAsync_WithNegativeButValidFeedback_ShouldApprove(string content, bool expectedApproval)
    {
        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.IsApproved.Should().Be(expectedApproval);
    }

    [Fact]
    public async Task ModerateContentAsync_ShouldSetCorrectMetadata()
    {
        // Arrange
        var content = "This is a test review content.";

        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.ContentLength.Should().Be(content.Length);
        result.ModeratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.ProcessingTime.Should().BeGreaterThan(TimeSpan.Zero);
        result.Confidence.Should().BeInRange(0, 1);
    }

    [Theory]
    [InlineData("f**k", "fuck")]
    [InlineData("sh*t", "shit")]
    [InlineData("a**", "ass")]
    [InlineData("f***", "fuck")]
    public async Task ModerateContentAsync_WithObfuscatedProfanity_ShouldDetect(string obfuscated, string detected)
    {
        // Arrange
        var content = $"This company is {obfuscated}";

        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.IsApproved.Should().BeFalse();
        result.FlaggedWords.Should().Contain(detected);
    }

    [Fact]
    public async Task ModerateContentAsync_WithMultipleEmails_ShouldDetectAll()
    {
        // Arrange
        var content = "Contact john@example.com or jane@test.com for more information";

        // Act
        var result = await _service.ModerateContentAsync(content);

        // Assert
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Contain("personal information");
        result.Categories.Should().Contain("PersonalInfo");
    }
}