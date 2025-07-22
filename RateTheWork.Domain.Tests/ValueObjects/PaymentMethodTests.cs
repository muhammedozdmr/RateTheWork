using FluentAssertions;
using RateTheWork.Domain.Tests.TestHelpers;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Tests.ValueObjects;

public class PaymentMethodTests : DomainTestBase
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreatePaymentMethod()
    {
        // Arrange
        var type = "card";
        var displayName = "Visa ending in 4242";
        var token = "pm_1234567890";
        var isDefault = true;

        // Act
        var paymentMethod = new PaymentMethod(type, displayName, token, isDefault);

        // Assert
        paymentMethod.Should().NotBeNull();
        paymentMethod.Type.Should().Be(type);
        paymentMethod.DisplayName.Should().Be(displayName);
        paymentMethod.Token.Should().Be(token);
        paymentMethod.IsDefault.Should().Be(isDefault);
    }

    [Theory]
    [InlineData(null, "Display", "token", true)]
    [InlineData("card", null, "token", true)]
    [InlineData("card", "Display", null, true)]
    public void Constructor_WithNullParameters_ShouldThrowException(
        string type, string displayName, string token, bool isDefault)
    {
        // Act
        var act = () => new PaymentMethod(type, displayName, token, isDefault);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("", "Display", "token", true)]
    [InlineData("card", "", "token", true)]
    [InlineData("card", "Display", "", true)]
    public void Constructor_WithEmptyParameters_ShouldThrowException(
        string type, string displayName, string token, bool isDefault)
    {
        // Act
        var act = () => new PaymentMethod(type, displayName, token, isDefault);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var method1 = new PaymentMethod("card", "Visa ending in 4242", "pm_123", true);
        var method2 = new PaymentMethod("card", "Visa ending in 4242", "pm_123", true);

        // Act & Assert
        method1.Should().Be(method2);
        method1.Equals(method2).Should().BeTrue();
        (method1 == method2).Should().BeTrue();
        (method1 != method2).Should().BeFalse();
    }

    [Theory]
    [InlineData("bank", "Visa ending in 4242", "pm_123", true)] // Different type
    [InlineData("card", "Mastercard ending in 5555", "pm_123", true)] // Different display name
    [InlineData("card", "Visa ending in 4242", "pm_456", true)] // Different token
    [InlineData("card", "Visa ending in 4242", "pm_123", false)] // Different isDefault
    public void Equals_WithDifferentValues_ShouldReturnFalse(
        string type, string displayName, string token, bool isDefault)
    {
        // Arrange
        var method1 = new PaymentMethod("card", "Visa ending in 4242", "pm_123", true);
        var method2 = new PaymentMethod(type, displayName, token, isDefault);

        // Act & Assert
        method1.Should().NotBe(method2);
        method1.Equals(method2).Should().BeFalse();
        (method1 == method2).Should().BeFalse();
        (method1 != method2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var paymentMethod = new PaymentMethod("card", "Visa ending in 4242", "pm_123", true);

        // Act & Assert
        paymentMethod.Equals(null).Should().BeFalse();
        (paymentMethod == null).Should().BeFalse();
        (null == paymentMethod).Should().BeFalse();
        (paymentMethod != null).Should().BeTrue();
        (null != paymentMethod).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var method1 = new PaymentMethod("card", "Visa ending in 4242", "pm_123", true);
        var method2 = new PaymentMethod("card", "Visa ending in 4242", "pm_123", true);

        // Act & Assert
        method1.GetHashCode().Should().Be(method2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var method1 = new PaymentMethod("card", "Visa ending in 4242", "pm_123", true);
        var method2 = new PaymentMethod("bank", "Bank Account", "ba_456", false);

        // Act & Assert
        method1.GetHashCode().Should().NotBe(method2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnDisplayName()
    {
        // Arrange
        var displayName = "Visa ending in 4242";
        var paymentMethod = new PaymentMethod("card", displayName, "pm_123", true);

        // Act
        var result = paymentMethod.ToString();

        // Assert
        result.Should().Be(displayName);
    }

    [Fact]
    public void ValueObject_ShouldBeImmutable()
    {
        // Arrange
        var paymentMethod = new PaymentMethod("card", "Visa ending in 4242", "pm_123", true);

        // Act & Assert
        // Properties should not have setters
        paymentMethod.GetType().GetProperty(nameof(PaymentMethod.Type))!.CanWrite.Should().BeFalse();
        paymentMethod.GetType().GetProperty(nameof(PaymentMethod.DisplayName))!.CanWrite.Should().BeFalse();
        paymentMethod.GetType().GetProperty(nameof(PaymentMethod.Token))!.CanWrite.Should().BeFalse();
        paymentMethod.GetType().GetProperty(nameof(PaymentMethod.IsDefault))!.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void GetComponents_ShouldReturnProtectedList()
    {
        // Arrange
        var paymentMethod = new PaymentMethod("card", "Visa ending in 4242", "pm_123", true);

        // Act
        var components = paymentMethod.GetEqualityComponents();

        // Assert
        components.Should().HaveCount(4);
        components.Should().ContainInOrder("card", "Visa ending in 4242", "pm_123", true);
    }

    [Theory]
    [InlineData("card")]
    [InlineData("bank")]
    [InlineData("paypal")]
    [InlineData("crypto")]
    public void Constructor_WithDifferentTypes_ShouldCreateValidPaymentMethod(string type)
    {
        // Act
        var paymentMethod = new PaymentMethod(type, "Test Display", "test_token", false);

        // Assert
        paymentMethod.Type.Should().Be(type);
    }

    [Fact]
    public void Constructor_WithNonDefaultPaymentMethod_ShouldSetIsDefaultToFalse()
    {
        // Act
        var paymentMethod = new PaymentMethod("card", "Visa ending in 4242", "pm_123", false);

        // Assert
        paymentMethod.IsDefault.Should().BeFalse();
    }
}