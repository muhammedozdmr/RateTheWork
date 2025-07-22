using FluentAssertions;
using RateTheWork.Domain.ValueObjects.Common;
using Xunit;

namespace RateTheWork.Domain.Tests.ValueObjects;

public class BasicValueObjectTests
{
    [Fact]
    public void Email_Create_Should_Create_Valid_Email()
    {
        // Arrange & Act
        var email = Email.Create("test@example.com");

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be("test@example.com");
        email.Domain.Should().Be("example.com");
        email.LocalPart.Should().Be("test");
    }

    [Fact]
    public void PhoneNumber_Create_Should_Create_Valid_PhoneNumber()
    {
        // Arrange & Act
        var phone = PhoneNumber.Create("05551234567");

        // Assert
        phone.Should().NotBeNull();
        phone.CountryCode.Should().Be("+90");
        phone.Number.Should().Be("5551234567");
    }

    [Fact]
    public void Money_Create_Should_Create_Valid_Money()
    {
        // Arrange & Act
        var money = Money.Create(100.50m, "TRY");

        // Assert
        money.Should().NotBeNull();
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("TRY");
    }

    [Fact]
    public void Rating_Create_Should_Create_Valid_Rating()
    {
        // Arrange & Act
        var rating = Rating.Create(4.5m);

        // Assert
        rating.Should().NotBeNull();
        rating.Value.Should().Be(4.5m);
        rating.Category.Should().Be("Overall");
    }

    [Fact]
    public void Money_Add_Should_Work_With_Same_Currency()
    {
        // Arrange
        var money1 = Money.Create(100, "TRY");
        var money2 = Money.Create(50, "TRY");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150);
        result.Currency.Should().Be("TRY");
    }

    [Fact]
    public void Rating_GetStarRating_Should_Return_Stars()
    {
        // Arrange
        var rating = Rating.Create(3.5m);

        // Act
        var stars = rating.GetStarRating();

        // Assert
        stars.Should().NotBeNullOrEmpty();
        stars.Should().Contain("★");
    }
}