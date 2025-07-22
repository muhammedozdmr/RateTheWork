using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;

namespace RateTheWork.Domain.Tests.TestHelpers;

public abstract class DomainTestBase
{
    protected IFixture Fixture { get; }

    protected DomainTestBase()
    {
        Fixture = new Fixture()
            .Customize(new AutoMoqCustomization());
        
        // Domain specific customizations
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    protected T Create<T>() => Fixture.Create<T>();
    
    protected Mock<T> CreateMock<T>() where T : class => Fixture.Freeze<Mock<T>>();
}