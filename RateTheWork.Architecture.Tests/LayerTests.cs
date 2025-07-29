using FluentAssertions;
using NetArchTest.Rules;

namespace RateTheWork.Architecture.Tests;

/// <summary>
/// Architecture tests for verifying layer structure and responsibilities.
/// Ensures each layer contains only the appropriate types.
/// </summary>
public class LayerTests
{
    [Fact]
    public void Domain_Should_Not_ContainServices()
    {
        // Arrange
        var assembly = typeof(Domain.Entities.User).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Service")
            .Should()
            .NotExist()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("Domain layer should not contain any services");
    }

    [Fact]
    public void Domain_Should_ContainOnlyPOCOs()
    {
        // Arrange
        var assembly = typeof(Domain.Entities.User).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .AreClasses()
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("Domain entities should be POCOs without EF Core dependencies");
    }

    [Fact]
    public void Application_Should_ContainHandlers()
    {
        // Arrange
        var assembly = typeof(Application.DependencyInjection).Assembly;

        // Act
        var commandHandlers = Types
            .InAssembly(assembly)
            .That()
            .HaveNameEndingWith("CommandHandler")
            .Should()
            .Exist()
            .GetResult();

        var queryHandlers = Types
            .InAssembly(assembly)
            .That()
            .HaveNameEndingWith("QueryHandler")
            .Should()
            .Exist()
            .GetResult();

        // Assert
        commandHandlers.IsSuccessful.Should().BeTrue("Application layer should contain command handlers");
        queryHandlers.IsSuccessful.Should().BeTrue("Application layer should contain query handlers");
    }

    [Fact]
    public void Infrastructure_Should_ContainRepositories()
    {
        // Arrange
        var assembly = typeof(Infrastructure.DependencyInjection).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .Exist()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("Infrastructure layer should contain repository implementations");
    }

    [Fact]
    public void Controllers_Should_ResideInApiLayer()
    {
        // Arrange
        var assembly = typeof(Api.Program).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .ResideInNamespace("RateTheWork.Api.Controllers")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All controllers should reside in Api.Controllers namespace");
    }

    [Fact]
    public void Application_Should_DefineInterfaces()
    {
        // Arrange
        var assembly = typeof(Application.DependencyInjection).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .AreInterfaces()
            .And()
            .ResideInNamespaceContaining("Interfaces")
            .Should()
            .Exist()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("Application layer should define interfaces");
    }

    [Fact]
    public void Infrastructure_Should_ImplementApplicationInterfaces()
    {
        // Arrange
        var infraAssembly = typeof(Infrastructure.DependencyInjection).Assembly;
        var appAssembly = typeof(Application.DependencyInjection).Assembly;

        // Act
        var interfaces = Types
            .InAssembly(appAssembly)
            .That()
            .AreInterfaces()
            .And()
            .ResideInNamespaceContaining("Interfaces")
            .GetTypes();

        var implementations = Types
            .InAssembly(infraAssembly)
            .That()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes();

        // Assert
        foreach (var @interface in interfaces)
        {
            var hasImplementation = implementations.Any(impl => 
                impl.GetInterfaces().Contains(@interface));

            if (@interface.Name.StartsWith("I") && !@interface.IsGenericTypeDefinition)
            {
                hasImplementation.Should().BeTrue($"Interface {@interface.Name} should have an implementation in Infrastructure layer");
            }
        }
    }

    [Fact]
    public void Handlers_Should_BeSealed()
    {
        // Arrange
        var assembly = typeof(Application.DependencyInjection).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All handlers should be sealed to prevent inheritance");
    }

    [Fact]
    public void ValueObjects_Should_BeSealed()
    {
        // Arrange
        var assembly = typeof(Domain.Entities.User).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .Inherit(typeof(Domain.Common.BaseValueObject))
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All value objects should be sealed");
    }
}