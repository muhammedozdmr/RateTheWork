using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Features.Subscriptions.Commands.CreateSubscription;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Application.Tests.Features.Subscriptions.Commands;

public class CreateSubscriptionCommandHandlerTests
{
    private readonly Mock<IRepository<Subscription>> _subscriptionRepositoryMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly CreateSubscriptionCommandHandler _handler;
    private readonly IFixture _fixture;

    public CreateSubscriptionCommandHandlerTests()
    {
        _subscriptionRepositoryMock = new Mock<IRepository<Subscription>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _fixture = new Fixture();

        _handler = new CreateSubscriptionCommandHandler(
            _subscriptionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateSubscription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateSubscriptionCommand
        {
            UserId = userId.ToString(),
            Plan = SubscriptionPlan.Professional,
            BillingCycle = BillingCycle.Monthly,
            StartTrial = true
        };

        var user = new User { Id = userId, Email = "test@example.com" };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _subscriptionRepositoryMock.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Subscription>)null);

        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.UserId.Should().Be(userId.ToString());
        result.Data.Plan.Should().Be(command.Plan.ToString());
        result.Data.BillingCycle.Should().Be(command.BillingCycle.ToString());
        result.Data.IsTrial.Should().Be(command.StartTrial);

        _subscriptionRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Subscription>(s => 
                s.UserId == userId.ToString() &&
                s.Plan == command.Plan &&
                s.BillingCycle == command.BillingCycle &&
                s.IsTrial == command.StartTrial),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateSubscriptionCommand
        {
            UserId = userId.ToString(),
            Plan = SubscriptionPlan.Basic,
            BillingCycle = BillingCycle.Monthly
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User not found");

        _subscriptionRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<Subscription>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingActiveSubscription_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateSubscriptionCommand
        {
            UserId = userId.ToString(),
            Plan = SubscriptionPlan.Professional,
            BillingCycle = BillingCycle.Yearly
        };

        var user = new User { Id = userId, Email = "test@example.com" };
        var existingSubscription = Subscription.Create(
            userId.ToString(),
            SubscriptionPlan.Basic,
            BillingCycle.Monthly);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _subscriptionRepositoryMock.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription> { existingSubscription });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already has an active subscription");

        _subscriptionRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<Subscription>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutTrial_ShouldCreatePaidSubscription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateSubscriptionCommand
        {
            UserId = userId.ToString(),
            Plan = SubscriptionPlan.Enterprise,
            BillingCycle = BillingCycle.Yearly,
            StartTrial = false,
            PaymentMethodToken = "pm_test_token"
        };

        var user = new User { Id = userId, Email = "test@example.com" };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _subscriptionRepositoryMock.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Subscription>)null);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.IsTrial.Should().BeFalse();

        _subscriptionRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Subscription>(s => 
                s.IsTrial == false &&
                s.Status == SubscriptionStatus.Active),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishDomainEvents()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateSubscriptionCommand
        {
            UserId = userId.ToString(),
            Plan = SubscriptionPlan.Professional,
            BillingCycle = BillingCycle.Monthly,
            StartTrial = true
        };

        var user = new User { Id = userId, Email = "test@example.com" };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _subscriptionRepositoryMock.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Subscription>)null);

        var capturedSubscription = (Subscription)null;
        _subscriptionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .Callback<Subscription, CancellationToken>((s, ct) => capturedSubscription = s)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedSubscription.Should().NotBeNull();
        capturedSubscription.DomainEvents.Should().NotBeEmpty();
        capturedSubscription.DomainEvents.Should().ContainItemsAssignableTo<Domain.Events.Subscription.SubscriptionCreatedEvent>();
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = "invalid-guid",
            Plan = SubscriptionPlan.Basic,
            BillingCycle = BillingCycle.Monthly
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid user ID");
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateSubscriptionCommand
        {
            UserId = userId.ToString(),
            Plan = SubscriptionPlan.Professional,
            BillingCycle = BillingCycle.Monthly
        };

        var user = new User { Id = userId, Email = "test@example.com" };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _subscriptionRepositoryMock.Setup(x => x.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Subscription>)null);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create subscription");
    }
}