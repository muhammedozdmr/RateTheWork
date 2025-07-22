using FluentAssertions;
using RateTheWork.Domain.Tests.TestHelpers;
using RateTheWork.Domain.ValueObjects;

namespace RateTheWork.Domain.Tests.ValueObjects;

public class AddressTests : DomainTestBase
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAddress()
    {
        // Arrange
        var country = "Turkey";
        var city = "Istanbul";
        var district = "Kadikoy";
        var street = "Bagdat Street";
        var postalCode = "34710";

        // Act
        var address = new Address(country, city, district, street, postalCode);

        // Assert
        address.Should().NotBeNull();
        address.Country.Should().Be(country);
        address.City.Should().Be(city);
        address.District.Should().Be(district);
        address.Street.Should().Be(street);
        address.PostalCode.Should().Be(postalCode);
    }

    [Theory]
    [InlineData(null, "Istanbul", "Kadikoy", "Street", "34710")]
    [InlineData("Turkey", null, "Kadikoy", "Street", "34710")]
    [InlineData("Turkey", "Istanbul", null, "Street", "34710")]
    [InlineData("Turkey", "Istanbul", "Kadikoy", null, "34710")]
    [InlineData("Turkey", "Istanbul", "Kadikoy", "Street", null)]
    public void Constructor_WithNullParameters_ShouldThrowException(
        string country, string city, string district, string street, string postalCode)
    {
        // Act
        var act = () => new Address(country, city, district, street, postalCode);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("", "Istanbul", "Kadikoy", "Street", "34710")]
    [InlineData("Turkey", "", "Kadikoy", "Street", "34710")]
    [InlineData("Turkey", "Istanbul", "", "Street", "34710")]
    [InlineData("Turkey", "Istanbul", "Kadikoy", "", "34710")]
    [InlineData("Turkey", "Istanbul", "Kadikoy", "Street", "")]
    public void Constructor_WithEmptyParameters_ShouldThrowException(
        string country, string city, string district, string street, string postalCode)
    {
        // Act
        var act = () => new Address(country, city, district, street, postalCode);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var address1 = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");
        var address2 = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");

        // Act & Assert
        address1.Should().Be(address2);
        address1.Equals(address2).Should().BeTrue();
        (address1 == address2).Should().BeTrue();
        (address1 != address2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var address1 = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");
        var address2 = new Address("Turkey", "Istanbul", "Besiktas", "Bagdat Street", "34710");

        // Act & Assert
        address1.Should().NotBe(address2);
        address1.Equals(address2).Should().BeFalse();
        (address1 == address2).Should().BeFalse();
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var address = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");

        // Act & Assert
        address.Equals(null).Should().BeFalse();
        (address == null).Should().BeFalse();
        (null == address).Should().BeFalse();
        (address != null).Should().BeTrue();
        (null != address).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var address = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");
        var other = "Not an address";

        // Act & Assert
        address.Equals(other).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var address1 = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");
        var address2 = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");

        // Act & Assert
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var address1 = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");
        var address2 = new Address("Turkey", "Ankara", "Cankaya", "Ataturk Street", "06690");

        // Act & Assert
        address1.GetHashCode().Should().NotBe(address2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnFormattedAddress()
    {
        // Arrange
        var address = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be("Bagdat Street, Kadikoy, Istanbul, 34710, Turkey");
    }

    [Fact]
    public void GetFullAddress_ShouldReturnCompleteAddress()
    {
        // Arrange
        var address = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");

        // Act
        var result = address.GetFullAddress();

        // Assert
        result.Should().Be("Bagdat Street, Kadikoy, Istanbul, 34710, Turkey");
    }

    [Fact]
    public void GetComponents_ShouldReturnProtectedList()
    {
        // Arrange
        var address = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");

        // Act
        var components = address.GetEqualityComponents();

        // Assert
        components.Should().HaveCount(5);
        components.Should().ContainInOrder("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");
    }

    [Fact]
    public void ValueObject_ShouldBeImmutable()
    {
        // Arrange
        var address = new Address("Turkey", "Istanbul", "Kadikoy", "Bagdat Street", "34710");

        // Act & Assert
        // Properties should not have setters
        address.GetType().GetProperty(nameof(Address.Country))!.CanWrite.Should().BeFalse();
        address.GetType().GetProperty(nameof(Address.City))!.CanWrite.Should().BeFalse();
        address.GetType().GetProperty(nameof(Address.District))!.CanWrite.Should().BeFalse();
        address.GetType().GetProperty(nameof(Address.Street))!.CanWrite.Should().BeFalse();
        address.GetType().GetProperty(nameof(Address.PostalCode))!.CanWrite.Should().BeFalse();
    }
}