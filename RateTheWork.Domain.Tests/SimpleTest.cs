using FluentAssertions;
using Xunit;

namespace RateTheWork.Domain.Tests;

public class SimpleTest
{
    [Fact]
    public void Test_Should_Pass()
    {
        // Arrange
        var expected = 4;
        
        // Act
        var result = 2 + 2;
        
        // Assert
        result.Should().Be(expected);
    }
}