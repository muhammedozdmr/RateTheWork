using FluentAssertions;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Subscription;
using RateTheWork.Domain.Events.CompanySubscription;
using RateTheWork.Domain.Exceptions.SubscriptionException;
using RateTheWork.Domain.Tests.TestHelpers;

namespace RateTheWork.Domain.Tests.Entities;

public class CompanySubscriptionTests : DomainTestBase
{
    private readonly Guid _companyId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidParameters_ShouldCreateCompanySubscription()
    {
        // Arrange
        var plan = SubscriptionPlan.CreateCompanyProfessional();
        var billingCycle = BillingCycle.Monthly;
        var billingContactId = Guid.NewGuid().ToString();

        // Act
        var subscription = CompanySubscription.Create(
            _companyId.ToString(),
            plan,
            billingCycle,
            billingContactId,
            startTrial: false);

        // Assert
        subscription.Should().NotBeNull();
        subscription.CompanyId.Should().Be(_companyId.ToString());
        subscription.Type.Should().Be(plan.Type);
        subscription.BillingCycle.Should().Be(billingCycle);
        subscription.IsActive.Should().BeTrue();
        subscription.DomainEvents.Should().ContainSingle(e => e is CompanySubscriptionCreatedEvent);
    }

    [Fact]
    public void Create_WithEmptyCompanyId_ShouldThrowException()
    {
        // Act
        var act = () => CompanySubscription.Create(
            string.Empty,
            SubscriptionPlan.CreateCompanyProfessional(),
            BillingCycle.Monthly,
            "billing-contact-id",
            false);

        // Assert
        act.Should().Throw<CompanySubscriptionException>()
            .WithMessage("Şirket ID boş olamaz");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidJobPostingQuota_ShouldThrowException(int invalidQuota)
    {
        // Act
        var act = () => CompanySubscription.Create(
            _companyId,
            CompanySubscriptionPlan.Professional,
            BillingCycle.Monthly,
            invalidQuota,
            5);

        // Assert
        act.Should().Throw<CompanySubscriptionException>()
            .WithMessage("Job posting quota must be greater than zero");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidHRPersonnelQuota_ShouldThrowException(int invalidQuota)
    {
        // Act
        var act = () => CompanySubscription.Create(
            _companyId,
            CompanySubscriptionPlan.Professional,
            BillingCycle.Monthly,
            10,
            invalidQuota);

        // Assert
        act.Should().Throw<CompanySubscriptionException>()
            .WithMessage("HR personnel quota must be greater than zero");
    }

    [Fact]
    public void ConsumeJobPostingQuota_WhenQuotaAvailable_ShouldConsumeQuota()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var initialQuota = subscription.JobPostingQuota;
        var initialUsed = subscription.UsedJobPostingQuota;

        // Act
        subscription.ConsumeJobPostingQuota();

        // Assert
        subscription.UsedJobPostingQuota.Should().Be(initialUsed + 1);
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionQuotaConsumedEvent);
    }

    [Fact]
    public void ConsumeJobPostingQuota_WhenQuotaExhausted_ShouldThrowException()
    {
        // Arrange
        var subscription = CompanySubscription.Create(
            _companyId,
            CompanySubscriptionPlan.Basic,
            BillingCycle.Monthly,
            2,
            5);

        // Consume all quota
        subscription.ConsumeJobPostingQuota();
        subscription.ConsumeJobPostingQuota();

        // Act
        var act = () => subscription.ConsumeJobPostingQuota();

        // Assert
        act.Should().Throw<CompanySubscriptionException>()
            .WithMessage("Job posting quota exceeded");
    }

    [Fact]
    public void ConsumeHRPersonnelQuota_WhenQuotaAvailable_ShouldConsumeQuota()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var initialUsed = subscription.UsedHRPersonnelQuota;

        // Act
        subscription.ConsumeHRPersonnelQuota();

        // Assert
        subscription.UsedHRPersonnelQuota.Should().Be(initialUsed + 1);
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionQuotaConsumedEvent);
    }

    [Fact]
    public void ConsumeHRPersonnelQuota_WhenQuotaExhausted_ShouldThrowException()
    {
        // Arrange
        var subscription = CompanySubscription.Create(
            _companyId,
            CompanySubscriptionPlan.Basic,
            BillingCycle.Monthly,
            10,
            2);

        // Consume all quota
        subscription.ConsumeHRPersonnelQuota();
        subscription.ConsumeHRPersonnelQuota();

        // Act
        var act = () => subscription.ConsumeHRPersonnelQuota();

        // Assert
        act.Should().Throw<CompanySubscriptionException>()
            .WithMessage("HR personnel quota exceeded");
    }

    [Fact]
    public void ReleaseJobPostingQuota_WhenUsedQuotaExists_ShouldReleaseQuota()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.ConsumeJobPostingQuota();
        var usedQuota = subscription.UsedJobPostingQuota;

        // Act
        subscription.ReleaseJobPostingQuota();

        // Assert
        subscription.UsedJobPostingQuota.Should().Be(usedQuota - 1);
    }

    [Fact]
    public void ReleaseJobPostingQuota_WhenNoUsedQuota_ShouldNotGoNegative()
    {
        // Arrange
        var subscription = CreateValidSubscription();

        // Act
        subscription.ReleaseJobPostingQuota();

        // Assert
        subscription.UsedJobPostingQuota.Should().Be(0);
    }

    [Fact]
    public void ReleaseHRPersonnelQuota_WhenUsedQuotaExists_ShouldReleaseQuota()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.ConsumeHRPersonnelQuota();
        var usedQuota = subscription.UsedHRPersonnelQuota;

        // Act
        subscription.ReleaseHRPersonnelQuota();

        // Assert
        subscription.UsedHRPersonnelQuota.Should().Be(usedQuota - 1);
    }

    [Fact]
    public void UpdateQuotas_WithValidQuotas_ShouldUpdateQuotas()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var newJobPostingQuota = 20;
        var newHRPersonnelQuota = 10;

        // Act
        subscription.UpdateQuotas(newJobPostingQuota, newHRPersonnelQuota);

        // Assert
        subscription.JobPostingQuota.Should().Be(newJobPostingQuota);
        subscription.HRPersonnelQuota.Should().Be(newHRPersonnelQuota);
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionQuotasUpdatedEvent);
    }

    [Fact]
    public void UpdateQuotas_WithQuotaLessThanUsed_ShouldAllowUpdate()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.ConsumeJobPostingQuota();
        subscription.ConsumeJobPostingQuota();
        subscription.ConsumeJobPostingQuota();

        // Act
        var act = () => subscription.UpdateQuotas(2, 10); // Set quota less than used (3)

        // Assert
        act.Should().NotThrow();
        subscription.JobPostingQuota.Should().Be(2);
    }

    [Fact]
    public void EnableAnalytics_ShouldEnableAnalytics()
    {
        // Arrange
        var subscription = CreateValidSubscription();

        // Act
        subscription.EnableAnalytics();

        // Assert
        subscription.AnalyticsEnabled.Should().BeTrue();
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionFeatureToggledEvent);
    }

    [Fact]
    public void DisableAnalytics_ShouldDisableAnalytics()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.EnableAnalytics();

        // Act
        subscription.DisableAnalytics();

        // Assert
        subscription.AnalyticsEnabled.Should().BeFalse();
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionFeatureToggledEvent);
    }

    [Fact]
    public void EnableReporting_ShouldEnableReporting()
    {
        // Arrange
        var subscription = CreateValidSubscription();

        // Act
        subscription.EnableReporting();

        // Assert
        subscription.ReportingEnabled.Should().BeTrue();
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionFeatureToggledEvent);
    }

    [Fact]
    public void DisableReporting_ShouldDisableReporting()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.EnableReporting();

        // Act
        subscription.DisableReporting();

        // Assert
        subscription.ReportingEnabled.Should().BeFalse();
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionFeatureToggledEvent);
    }

    [Fact]
    public void EnablePrioritySupport_ShouldEnablePrioritySupport()
    {
        // Arrange
        var subscription = CreateValidSubscription();

        // Act
        subscription.EnablePrioritySupport();

        // Assert
        subscription.PrioritySupportEnabled.Should().BeTrue();
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionFeatureToggledEvent);
    }

    [Fact]
    public void DisablePrioritySupport_ShouldDisablePrioritySupport()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.EnablePrioritySupport();

        // Act
        subscription.DisablePrioritySupport();

        // Assert
        subscription.PrioritySupportEnabled.Should().BeFalse();
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionFeatureToggledEvent);
    }

    [Fact]
    public void Renew_WhenActive_ShouldRenewSubscription()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var oldExpiresAt = subscription.ExpiresAt;

        // Act
        subscription.Renew();

        // Assert
        subscription.ExpiresAt.Should().BeAfter(oldExpiresAt);
        subscription.LastRenewalDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionRenewedEvent);
    }

    [Fact]
    public void Renew_ShouldResetQuotas()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.ConsumeJobPostingQuota();
        subscription.ConsumeJobPostingQuota();
        subscription.ConsumeHRPersonnelQuota();

        // Act
        subscription.Renew();

        // Assert
        subscription.UsedJobPostingQuota.Should().Be(0);
        subscription.UsedHRPersonnelQuota.Should().Be(0);
    }

    [Fact]
    public void Cancel_WhenActive_ShouldCancelSubscription()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var reason = "No longer needed";

        // Act
        subscription.Cancel(reason);

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        subscription.CancellationReason.Should().Be(reason);
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionCancelledEvent);
    }

    [Fact]
    public void Suspend_WhenActive_ShouldSuspendSubscription()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var reason = "Payment failed";

        // Act
        subscription.Suspend(reason);

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Suspended);
        subscription.SuspendedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        subscription.SuspensionReason.Should().Be(reason);
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionSuspendedEvent);
    }

    [Fact]
    public void Reactivate_WhenSuspended_ShouldReactivateSubscription()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.Suspend("Payment failed");
        subscription.ClearDomainEvents();

        // Act
        subscription.Reactivate();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionReactivatedEvent);
    }

    [Fact]
    public void ChangePlan_WithValidPlan_ShouldChangePlan()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var newPlan = CompanySubscriptionPlan.Enterprise;

        // Act
        subscription.ChangePlan(newPlan);

        // Assert
        subscription.Plan.Should().Be(newPlan);
        subscription.DomainEvents.Should().Contain(e => e is CompanySubscriptionPlanChangedEvent);
    }

    [Fact]
    public void ChangePlan_ToSamePlan_ShouldNotAddEvent()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.ClearDomainEvents();

        // Act
        subscription.ChangePlan(subscription.Plan);

        // Assert
        subscription.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void GetAvailableJobPostingQuota_ShouldReturnCorrectValue()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.ConsumeJobPostingQuota();
        subscription.ConsumeJobPostingQuota();

        // Act
        var available = subscription.GetAvailableJobPostingQuota();

        // Assert
        available.Should().Be(subscription.JobPostingQuota - subscription.UsedJobPostingQuota);
    }

    [Fact]
    public void GetAvailableHRPersonnelQuota_ShouldReturnCorrectValue()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.ConsumeHRPersonnelQuota();
        subscription.ConsumeHRPersonnelQuota();
        subscription.ConsumeHRPersonnelQuota();

        // Act
        var available = subscription.GetAvailableHRPersonnelQuota();

        // Assert
        available.Should().Be(subscription.HRPersonnelQuota - subscription.UsedHRPersonnelQuota);
    }

    private CompanySubscription CreateValidSubscription()
    {
        return CompanySubscription.Create(
            _companyId,
            CompanySubscriptionPlan.Professional,
            BillingCycle.Monthly,
            10,
            5);
    }
}