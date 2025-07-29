using FluentAssertions;
using NetArchTest.Rules;

namespace RateTheWork.Architecture.Tests;

/// <summary>
/// Architecture tests for verifying dependency rules between layers.
/// Ensures that the clean architecture principles are followed.
/// </summary>
public class DependencyTests
{
    private const string DomainNamespace = "RateTheWork.Domain";
    private const string ApplicationNamespace = "RateTheWork.Application";
    private const string InfrastructureNamespace = "RateTheWork.Infrastructure";
    private const string ApiNamespace = "RateTheWork.Api";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnOtherProjects()
    {
        // Arrange
        var assembly = typeof(Domain.Entities.User).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAll(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOnInfrastructure()
    {
        // Arrange
        var assembly = typeof(Application.DependencyInjection).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOnApi()
    {
        // Arrange
        var assembly = typeof(Application.DependencyInjection).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Handlers_Should_Have_DependencyOnDomain()
    {
        // Arrange
        var assembly = typeof(Application.DependencyInjection).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .HaveDependencyOn(DomainNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_Should_HaveDependencyOnApplication()
    {
        // Arrange
        var assembly = typeof(Infrastructure.DependencyInjection).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Or()
            .HaveNameEndingWith("Service")
            .Should()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}