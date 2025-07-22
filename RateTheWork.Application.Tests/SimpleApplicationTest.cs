using FluentAssertions;
using Xunit;

namespace RateTheWork.Application.Tests;

public class SimpleApplicationTest
{
    [Fact]
    public void Application_Test_Should_Pass()
    {
        // Arrange
        var input = "test";
        
        // Act
        var result = input.ToUpper();
        
        // Assert
        result.Should().Be("TEST");
    }
    
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(10, 20, 30)]
    [InlineData(-5, 5, 0)]
    public void Addition_Should_Work_Correctly(int a, int b, int expected)
    {
        // Act
        var result = a + b;
        
        // Assert
        result.Should().Be(expected);
    }
}