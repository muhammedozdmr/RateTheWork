using FluentAssertions;
using RateTheWork.Domain.Tests.TestHelpers;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Tests.ValueObjects;

public class SubscriptionFeatureTests : DomainTestBase
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateFeature()
    {
        // Arrange
        var code = "analytics";
        var name = "Advanced Analytics";
        var isEnabled = true;
        var usageLimit = 1000;

        // Act
        var feature = new SubscriptionFeature(code, name, isEnabled, usageLimit);

        // Assert
        feature.Should().NotBeNull();
        feature.Code.Should().Be(code);
        feature.Name.Should().Be(name);
        feature.IsEnabled.Should().Be(isEnabled);
        feature.UsageLimit.Should().Be(usageLimit);
    }

    [Fact]
    public void Constructor_WithoutUsageLimit_ShouldCreateUnlimitedFeature()
    {
        // Arrange
        var code = "export";
        var name = "Data Export";
        var isEnabled = true;

        // Act
        var feature = new SubscriptionFeature(code, name, isEnabled, null);

        // Assert
        feature.Should().NotBeNull();
        feature.UsageLimit.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "Name", true, 100)]
    [InlineData("code", null, true, 100)]
    public void Constructor_WithNullRequiredParameters_ShouldThrowException(
        string code, string name, bool isEnabled, int? usageLimit)
    {
        // Act
        var act = () => new SubscriptionFeature(code, name, isEnabled, usageLimit);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("", "Name", true, 100)]
    [InlineData("code", "", true, 100)]
    public void Constructor_WithEmptyRequiredParameters_ShouldThrowException(
        string code, string name, bool isEnabled, int? usageLimit)
    {
        // Act
        var act = () => new SubscriptionFeature(code, name, isEnabled, usageLimit);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithNegativeUsageLimit_ShouldThrowException(int negativeLimit)
    {
        // Act
        var act = () => new SubscriptionFeature("code", "Name", true, negativeLimit);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Usage limit cannot be negative");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var feature1 = new SubscriptionFeature("analytics", "Advanced Analytics", true, 1000);
        var feature2 = new SubscriptionFeature("analytics", "Advanced Analytics", true, 1000);

        // Act & Assert
        feature1.Should().Be(feature2);
        feature1.Equals(feature2).Should().BeTrue();
        (feature1 == feature2).Should().BeTrue();
        (feature1 != feature2).Should().BeFalse();
    }

    [Theory]
    [InlineData("reporting", "Advanced Analytics", true, 1000)] // Different code
    [InlineData("analytics", "Basic Analytics", true, 1000)] // Different name
    [InlineData("analytics", "Advanced Analytics", false, 1000)] // Different isEnabled
    [InlineData("analytics", "Advanced Analytics", true, 2000)] // Different usageLimit
    [InlineData("analytics", "Advanced Analytics", true, null)] // Different usageLimit (null)
    public void Equals_WithDifferentValues_ShouldReturnFalse(
        string code, string name, bool isEnabled, int? usageLimit)
    {
        // Arrange
        var feature1 = new SubscriptionFeature("analytics", "Advanced Analytics", true, 1000);
        var feature2 = new SubscriptionFeature(code, name, isEnabled, usageLimit);

        // Act & Assert
        feature1.Should().NotBe(feature2);
        feature1.Equals(feature2).Should().BeFalse();
        (feature1 == feature2).Should().BeFalse();
        (feature1 != feature2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithBothNullUsageLimit_ShouldReturnTrue()
    {
        // Arrange
        var feature1 = new SubscriptionFeature("export", "Data Export", true, null);
        var feature2 = new SubscriptionFeature("export", "Data Export", true, null);

        // Act & Assert
        feature1.Should().Be(feature2);
        feature1.Equals(feature2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var feature = new SubscriptionFeature("analytics", "Advanced Analytics", true, 1000);

        // Act & Assert
        feature.Equals(null).Should().BeFalse();
        (feature == null).Should().BeFalse();
        (null == feature).Should().BeFalse();
        (feature != null).Should().BeTrue();
        (null != feature).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var feature1 = new SubscriptionFeature("analytics", "Advanced Analytics", true, 1000);
        var feature2 = new SubscriptionFeature("analytics", "Advanced Analytics", true, 1000);

        // Act & Assert
        feature1.GetHashCode().Should().Be(feature2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var feature1 = new SubscriptionFeature("analytics", "Advanced Analytics", true, 1000);
        var feature2 = new SubscriptionFeature("export", "Data Export", false, null);

        // Act & Assert
        feature1.GetHashCode().Should().NotBe(feature2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var feature = new SubscriptionFeature("analytics", "Advanced Analytics", true, 1000);

        // Act
        var result = feature.ToString();

        // Assert
        result.Should().Be("Advanced Analytics (analytics) - Enabled - Limit: 1000");
    }

    [Fact]
    public void ToString_WhenDisabled_ShouldIndicateDisabled()
    {
        // Arrange
        var feature = new SubscriptionFeature("analytics", "Advanced Analytics", false, 1000);

        // Act
        var result = feature.ToString();

        // Assert
        result.Should().Be("Advanced Analytics (analytics) - Disabled - Limit: 1000");
    }

    [Fact]
    public void ToString_WithNoLimit_ShouldIndicateUnlimited()
    {
        // Arrange
        var feature = new SubscriptionFeature("export", "Data Export", true, null);

        // Act
        var result = feature.ToString();

        // Assert
        result.Should().Be("Data Export (export) - Enabled - Limit: Unlimited");
    }

    [Fact]
    public void ValueObject_ShouldBeImmutable()
    {
        // Arrange
        var feature = new SubscriptionFeature("analytics", "Advanced Analytics", true, 1000);

        // Act & Assert
        // Properties should not have setters
        feature.GetType().GetProperty(nameof(SubscriptionFeature.Code))!.CanWrite.Should().BeFalse();
        feature.GetType().GetProperty(nameof(SubscriptionFeature.Name))!.CanWrite.Should().BeFalse();
        feature.GetType().GetProperty(nameof(SubscriptionFeature.IsEnabled))!.CanWrite.Should().BeFalse();
        feature.GetType().GetProperty(nameof(SubscriptionFeature.UsageLimit))!.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void GetComponents_ShouldReturnProtectedList()
    {
        // Arrange
        var feature = new SubscriptionFeature("analytics", "Advanced Analytics", true, 1000);

        // Act
        var components = feature.GetEqualityComponents();

        // Assert
        components.Should().HaveCount(4);
        components.Should().ContainInOrder("analytics", "Advanced Analytics", true, 1000);
    }

    [Fact]
    public void GetComponents_WithNullUsageLimit_ShouldIncludeNull()
    {
        // Arrange
        var feature = new SubscriptionFeature("export", "Data Export", true, null);

        // Act
        var components = feature.GetEqualityComponents();

        // Assert
        components.Should().HaveCount(4);
        components.Should().ContainInOrder("export", "Data Export", true, null);
    }

    [Theory]
    [InlineData("api_calls", "API Calls")]
    [InlineData("storage_gb", "Storage (GB)")]
    [InlineData("users", "User Accounts")]
    [InlineData("custom_reports", "Custom Reports")]
    public void Constructor_WithVariousFeatureCodes_ShouldCreateValidFeatures(string code, string name)
    {
        // Act
        var feature = new SubscriptionFeature(code, name, true, 100);

        // Assert
        feature.Code.Should().Be(code);
        feature.Name.Should().Be(name);
    }

    [Fact]
    public void Constructor_WithZeroUsageLimit_ShouldCreateFeature()
    {
        // Act
        var feature = new SubscriptionFeature("trial", "Trial Feature", true, 0);

        // Assert
        feature.UsageLimit.Should().Be(0);
    }
}