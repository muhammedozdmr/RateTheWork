using FluentAssertions;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Subscription;
using RateTheWork.Domain.Events.Subscription;
using RateTheWork.Domain.Exceptions.SubscriptionException;
using RateTheWork.Domain.Tests.TestHelpers;
using RateTheWork.Domain.ValueObjects;
using RateTheWork.Domain.ValueObjects.Subscription;

namespace RateTheWork.Domain.Tests.Entities;

public class SubscriptionTests : DomainTestBase
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateSubscription()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var plan = SubscriptionPlan.CreateCompanyProfessional();
        var billingCycle = BillingCycle.Monthly;

        // Act
        var subscription = Subscription.Create(userId, plan, billingCycle);

        // Assert
        subscription.Should().NotBeNull();
        subscription.UserId.Should().Be(userId);
        subscription.Type.Should().Be(plan.Type);
        subscription.BillingCycle.Should().Be(billingCycle);
        subscription.IsActive.Should().BeTrue();
        subscription.IsTrialPeriod.Should().BeFalse();
        subscription.DomainEvents.Should().ContainSingle(e => e is SubscriptionCreatedEvent);
    }

    [Fact]
    public void Create_WithTrial_ShouldCreateTrialSubscription()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var plan = SubscriptionPlan.CreateCompanyBasic();
        var billingCycle = BillingCycle.Monthly;

        // Act
        var subscription = Subscription.Create(userId, plan, billingCycle, startTrial: true);

        // Assert
        subscription.Should().NotBeNull();
        subscription.IsTrialPeriod.Should().BeTrue();
        subscription.IsActive.Should().BeTrue();
        subscription.TrialEndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(plan.TrialDays), TimeSpan.FromMinutes(1));
        subscription.DomainEvents.Should().ContainSingle(e => e is SubscriptionCreatedEvent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidUserId_ShouldThrowException(string invalidUserId)
    {
        // Act
        var act = () => Subscription.Create(invalidUserId, SubscriptionPlan.CreateCompanyBasic(), BillingCycle.Monthly);

        // Assert
        act.Should().Throw<SubscriptionException>()
            .WithMessage("Kullanıcı ID boş olamaz");
    }

    [Fact]
    public void Cancel_WhenActive_ShouldCancelSubscription()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid().ToString(), SubscriptionPlan.CreateCompanyProfessional(), BillingCycle.Monthly);
        var reason = "No longer needed";

        // Act
        subscription.Cancel(reason);

        // Assert
        subscription.IsCancelled.Should().BeTrue();
        subscription.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        subscription.CancellationReason.Should().Be(reason);
        subscription.DomainEvents.Should().Contain(e => e is SubscriptionCancelledEvent);
    }

    [Fact]
    public void Renew_WhenActiveAndNotTrial_ShouldRenewSubscription()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid().ToString(), SubscriptionPlan.CreateCompanyProfessional(), BillingCycle.Monthly);
        var oldNextBillingDate = subscription.NextBillingDate;

        // Act
        subscription.Renew();

        // Assert
        subscription.NextBillingDate.Should().BeAfter(oldNextBillingDate ?? DateTime.UtcNow);
        subscription.DomainEvents.Should().Contain(e => e is SubscriptionRenewedEvent);
    }

    [Fact]
    public void Renew_WhenTrial_ShouldThrowException()
    {
        // Arrange
        var subscription = Subscription.Create(Guid.NewGuid().ToString(), SubscriptionPlan.CreateCompanyBasic(), BillingCycle.Monthly, startTrial: true);

        // Act
        var act = () => subscription.Renew();

        // Assert
        act.Should().Throw<SubscriptionException>()
            .WithMessage("Deneme sürümündeki üyelikler yenilenemez");
    }

    [Fact]
    public void CheckFeature_WithEnabledFeature_ShouldReturnTrue()
    {
        // Arrange
        var plan = SubscriptionPlan.CreateCompanyProfessional();
        var subscription = Subscription.Create(Guid.NewGuid().ToString(), plan, BillingCycle.Monthly);

        // Act
        var hasFeature = subscription.HasFeature(FeatureType.PostJobListing);

        // Assert
        hasFeature.Should().BeTrue();
    }

    [Fact]
    public void CheckFeature_WithDisabledFeature_ShouldReturnFalse()
    {
        // Arrange
        var plan = SubscriptionPlan.CreateCompanyBasic();
        var subscription = Subscription.Create(Guid.NewGuid().ToString(), plan, BillingCycle.Monthly);

        // Act
        var hasFeature = subscription.HasFeature(FeatureType.APIAccess);

        // Assert
        hasFeature.Should().BeFalse();
    }

    [Fact]
    public void CheckUsageLimit_ShouldReturnCorrectLimit()
    {
        // Arrange
        var plan = SubscriptionPlan.CreateCompanyBasic();
        var subscription = Subscription.Create(Guid.NewGuid().ToString(), plan, BillingCycle.Monthly);

        // Act
        var limit = subscription.GetUsageLimit("MonthlyJobPostings");

        // Assert
        limit.Should().Be(5);
    }

    [Fact]
    public void RecordUsage_WithValidFeature_ShouldUpdateUsage()
    {
        // Arrange
        var plan = SubscriptionPlan.CreateCompanyBasic();
        var subscription = Subscription.Create(Guid.NewGuid().ToString(), plan, BillingCycle.Monthly);

        // Act
        subscription.RecordUsage("MonthlyJobPostings", 1);

        // Assert
        subscription.GetCurrentUsage("MonthlyJobPostings").Should().Be(1);
    }

    [Fact]
    public void RecordUsage_ExceedingLimit_ShouldThrowException()
    {
        // Arrange
        var plan = SubscriptionPlan.CreateCompanyBasic();
        var subscription = Subscription.Create(Guid.NewGuid().ToString(), plan, BillingCycle.Monthly);
        
        // Use up the limit
        for (int i = 0; i < 5; i++)
        {
            subscription.RecordUsage("MonthlyJobPostings", 1);
        }

        // Act
        var act = () => subscription.RecordUsage("MonthlyJobPostings", 1);

        // Assert
        act.Should().Throw<SubscriptionException>()
            .WithMessage("Kullanım limiti aşıldı: MonthlyJobPostings");
    }

    [Fact]
    public void ConvertToTrialToFull_ShouldUpdateSubscriptionStatus()
    {
        // Arrange
        var subscription = Subscription.Create(
            Guid.NewGuid().ToString(), 
            SubscriptionPlan.CreateCompanyBasic(), 
            BillingCycle.Monthly, 
            startTrial: true);

        // Act
        subscription.ConvertTrialToFull("pm_test123");

        // Assert
        subscription.IsTrialPeriod.Should().BeFalse();
        subscription.PaymentMethodId.Should().Be("pm_test123");
        subscription.DomainEvents.Should().Contain(e => e is SubscriptionTrialConvertedEvent);
    }
}