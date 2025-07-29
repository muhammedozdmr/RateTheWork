using FluentValidation;
using RateTheWork.Domain.Enums.Subscription;

namespace RateTheWork.Application.Features.Subscriptions.Commands.CreateSubscription;

/// <summary>
/// Üyelik oluşturma validasyonu
/// </summary>
public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .Length(1, 50).WithMessage("User ID must be between 1 and 50 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid subscription type");

        RuleFor(x => x.BillingCycle)
            .IsInEnum().WithMessage("Invalid billing cycle");

        RuleFor(x => x.PaymentMethodId)
            .NotEmpty().When(x => !x.StartTrial && x.Type != SubscriptionType.Free)
            .WithMessage("Payment method is required for paid subscriptions");

        RuleFor(x => x.PaymentMethodId)
            .Empty().When(x => x.StartTrial)
            .WithMessage("Payment method should not be provided for trial subscriptions");

        RuleFor(x => x.Type)
            .NotEqual(SubscriptionType.CompanyEnterprise)
            .WithMessage("Enterprise subscriptions must be created through company subscription process");
    }
}
