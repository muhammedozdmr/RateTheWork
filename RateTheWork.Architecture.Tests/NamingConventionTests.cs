using FluentAssertions;
using NetArchTest.Rules;

namespace RateTheWork.Architecture.Tests;

/// <summary>
/// Architecture tests for verifying naming conventions.
/// Ensures consistent naming patterns across the codebase.
/// </summary>
public class NamingConventionTests
{
    [Fact]
    public void Interfaces_Should_StartWithI()
    {
        // Arrange
        var assemblies = new[]
        {
            typeof(Domain.Entities.User).Assembly,
            typeof(Application.DependencyInjection).Assembly,
            typeof(Infrastructure.DependencyInjection).Assembly
        };

        foreach (var assembly in assemblies)
        {
            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue($"All interfaces in {assembly.GetName().Name} should start with 'I'");
        }
    }

    [Fact]
    public void Classes_Should_NotStartWithI()
    {
        // Arrange
        var assemblies = new[]
        {
            typeof(Domain.Entities.User).Assembly,
            typeof(Application.DependencyInjection).Assembly,
            typeof(Infrastructure.DependencyInjection).Assembly
        };

        foreach (var assembly in assemblies)
        {
            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .AreClasses()
                .ShouldNot()
                .HaveNameStartingWith("I")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue($"No classes in {assembly.GetName().Name} should start with 'I'");
        }
    }

    [Fact]
    public void CommandHandlers_Should_EndWithCommandHandler()
    {
        // Arrange
        var assembly = typeof(Application.DependencyInjection).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace("RateTheWork.Application.Features")
            .And()
            .HaveNameContaining("Command")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All command handlers should end with 'CommandHandler'");
    }

    [Fact]
    public void QueryHandlers_Should_EndWithQueryHandler()
    {
        // Arrange
        var assembly = typeof(Application.DependencyInjection).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .ResideInNamespace("RateTheWork.Application.Features")
            .And()
            .HaveNameContaining("Query")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All query handlers should end with 'QueryHandler'");
    }

    [Fact]
    public void Repositories_Should_EndWithRepository()
    {
        // Arrange
        var assembly = typeof(Infrastructure.DependencyInjection).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(Application.Common.Interfaces.Persistence.IGenericRepository<>))
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All repository implementations should end with 'Repository'");
    }

    [Fact]
    public void Services_Should_EndWithService()
    {
        // Arrange
        var assemblies = new[]
        {
            typeof(Application.DependencyInjection).Assembly,
            typeof(Infrastructure.DependencyInjection).Assembly
        };

        foreach (var assembly in assemblies)
        {
            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining("Services")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .Should()
                .HaveNameEndingWith("Service")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue($"All service implementations in {assembly.GetName().Name} should end with 'Service'");
        }
    }

    [Fact]
    public void ValueObjects_Should_ResideInValueObjectsNamespace()
    {
        // Arrange
        var assembly = typeof(Domain.Entities.User).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .Inherit(typeof(Domain.Common.BaseValueObject))
            .Should()
            .ResideInNamespaceContaining("ValueObjects")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All value objects should reside in ValueObjects namespace");
    }

    [Fact]
    public void Entities_Should_ResideInEntitiesNamespace()
    {
        // Arrange
        var assembly = typeof(Domain.Entities.User).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .Inherit(typeof(Domain.Common.BaseEntity))
            .Should()
            .ResideInNamespace("RateTheWork.Domain.Entities")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All entities should reside in Entities namespace");
    }
}