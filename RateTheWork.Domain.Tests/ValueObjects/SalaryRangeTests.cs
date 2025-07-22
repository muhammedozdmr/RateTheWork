using FluentAssertions;
using RateTheWork.Domain.Enums.JobPosting;
using RateTheWork.Domain.Tests.TestHelpers;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Tests.ValueObjects;

public class SalaryRangeTests : DomainTestBase
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateSalaryRange()
    {
        // Arrange
        var min = 50000m;
        var max = 80000m;
        var currency = "USD";
        var period = SalaryPeriod.Yearly;

        // Act
        var salaryRange = new SalaryRange(min, max, currency, period);

        // Assert
        salaryRange.Should().NotBeNull();
        salaryRange.Min.Should().Be(min);
        salaryRange.Max.Should().Be(max);
        salaryRange.Currency.Should().Be(currency);
        salaryRange.Period.Should().Be(period);
    }

    [Theory]
    [InlineData(-1, 50000)]
    [InlineData(0, -1)]
    [InlineData(-1000, -500)]
    public void Constructor_WithNegativeValues_ShouldThrowException(decimal min, decimal max)
    {
        // Act
        var act = () => new SalaryRange(min, max, "USD", SalaryPeriod.Yearly);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Salary values cannot be negative");
    }

    [Fact]
    public void Constructor_WithMinGreaterThanMax_ShouldThrowException()
    {
        // Act
        var act = () => new SalaryRange(80000m, 50000m, "USD", SalaryPeriod.Yearly);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Minimum salary cannot be greater than maximum salary");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidCurrency_ShouldThrowException(string invalidCurrency)
    {
        // Act
        var act = () => new SalaryRange(50000m, 80000m, invalidCurrency, SalaryPeriod.Yearly);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Currency is required");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedRange()
    {
        // Arrange
        var salaryRange = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);

        // Act
        var result = salaryRange.ToString();

        // Assert
        result.Should().Be("USD 50000 - 80000 per Year");
    }

    [Fact]
    public void ToString_WithMonthlyPeriod_ShouldFormatCorrectly()
    {
        // Arrange
        var salaryRange = new SalaryRange(4000m, 6000m, "EUR", SalaryPeriod.Monthly);

        // Act
        var result = salaryRange.ToString();

        // Assert
        result.Should().Be("EUR 4000 - 6000 per Month");
    }

    [Fact]
    public void ToString_WithHourlyPeriod_ShouldFormatCorrectly()
    {
        // Arrange
        var salaryRange = new SalaryRange(25m, 40m, "GBP", SalaryPeriod.Hourly);

        // Act
        var result = salaryRange.ToString();

        // Assert
        result.Should().Be("GBP 25 - 40 per Hour");
    }

    [Fact]
    public void IsInRange_WithValueInRange_ShouldReturnTrue()
    {
        // Arrange
        var salaryRange = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);

        // Act & Assert
        salaryRange.IsInRange(60000m).Should().BeTrue();
        salaryRange.IsInRange(50000m).Should().BeTrue(); // Min boundary
        salaryRange.IsInRange(80000m).Should().BeTrue(); // Max boundary
    }

    [Fact]
    public void IsInRange_WithValueOutOfRange_ShouldReturnFalse()
    {
        // Arrange
        var salaryRange = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);

        // Act & Assert
        salaryRange.IsInRange(49999m).Should().BeFalse();
        salaryRange.IsInRange(80001m).Should().BeFalse();
        salaryRange.IsInRange(0m).Should().BeFalse();
        salaryRange.IsInRange(100000m).Should().BeFalse();
    }

    [Fact]
    public void GetMidpoint_ShouldReturnCorrectValue()
    {
        // Arrange
        var salaryRange = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);

        // Act
        var midpoint = salaryRange.GetMidpoint();

        // Assert
        midpoint.Should().Be(65000m);
    }

    [Fact]
    public void GetMidpoint_WithEqualMinMax_ShouldReturnSameValue()
    {
        // Arrange
        var salaryRange = new SalaryRange(60000m, 60000m, "USD", SalaryPeriod.Yearly);

        // Act
        var midpoint = salaryRange.GetMidpoint();

        // Assert
        midpoint.Should().Be(60000m);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var range1 = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);
        var range2 = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);

        // Act & Assert
        range1.Should().Be(range2);
        range1.Equals(range2).Should().BeTrue();
        (range1 == range2).Should().BeTrue();
        (range1 != range2).Should().BeFalse();
    }

    [Theory]
    [InlineData(50000, 80001, "USD", SalaryPeriod.Yearly)] // Different max
    [InlineData(50001, 80000, "USD", SalaryPeriod.Yearly)] // Different min
    [InlineData(50000, 80000, "EUR", SalaryPeriod.Yearly)] // Different currency
    [InlineData(50000, 80000, "USD", SalaryPeriod.Monthly)] // Different period
    public void Equals_WithDifferentValues_ShouldReturnFalse(
        decimal min, decimal max, string currency, SalaryPeriod period)
    {
        // Arrange
        var range1 = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);
        var range2 = new SalaryRange(min, max, currency, period);

        // Act & Assert
        range1.Should().NotBe(range2);
        range1.Equals(range2).Should().BeFalse();
        (range1 == range2).Should().BeFalse();
        (range1 != range2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var salaryRange = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);

        // Act & Assert
        salaryRange.Equals(null).Should().BeFalse();
        (salaryRange == null).Should().BeFalse();
        (null == salaryRange).Should().BeFalse();
        (salaryRange != null).Should().BeTrue();
        (null != salaryRange).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var range1 = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);
        var range2 = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);

        // Act & Assert
        range1.GetHashCode().Should().Be(range2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var range1 = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);
        var range2 = new SalaryRange(60000m, 90000m, "EUR", SalaryPeriod.Monthly);

        // Act & Assert
        range1.GetHashCode().Should().NotBe(range2.GetHashCode());
    }

    [Fact]
    public void ValueObject_ShouldBeImmutable()
    {
        // Arrange
        var salaryRange = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);

        // Act & Assert
        // Properties should not have setters
        salaryRange.GetType().GetProperty(nameof(SalaryRange.Min))!.CanWrite.Should().BeFalse();
        salaryRange.GetType().GetProperty(nameof(SalaryRange.Max))!.CanWrite.Should().BeFalse();
        salaryRange.GetType().GetProperty(nameof(SalaryRange.Currency))!.CanWrite.Should().BeFalse();
        salaryRange.GetType().GetProperty(nameof(SalaryRange.Period))!.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void GetComponents_ShouldReturnProtectedList()
    {
        // Arrange
        var salaryRange = new SalaryRange(50000m, 80000m, "USD", SalaryPeriod.Yearly);

        // Act
        var components = salaryRange.GetEqualityComponents();

        // Assert
        components.Should().HaveCount(4);
        components.Should().ContainInOrder(50000m, 80000m, "USD", SalaryPeriod.Yearly);
    }
}