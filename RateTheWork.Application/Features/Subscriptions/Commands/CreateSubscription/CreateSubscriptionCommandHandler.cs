using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Models;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.ValueObjects.Subscription;

namespace RateTheWork.Application.Features.Subscriptions.Commands.CreateSubscription;

/// <summary>
/// Üyelik oluşturma handler
/// </summary>
public class
    CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Result<CreateSubscriptionResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IMapper _mapper;

    public CreateSubscriptionCommandHandler
    (
        IApplicationDbContext context
        , IMapper mapper
        , IDateTimeService dateTimeService
        , ICurrentUserService currentUserService
    )
    {
        _context = context;
        _mapper = mapper;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CreateSubscriptionResponse>> Handle
    (
        CreateSubscriptionCommand request
        , CancellationToken cancellationToken
    )
    {
        // Kullanıcının aktif üyeliği var mı kontrol et
        var existingSubscription = await _context.Set<Subscription>()
            .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.IsActive, cancellationToken);

        if (existingSubscription != null)
        {
            return Result<CreateSubscriptionResponse>.Failure("User already has an active subscription");
        }

        // Deneme süresi kullanılmış mı kontrol et
        if (request.StartTrial)
        {
            var hasUsedTrial = await _context.Set<Subscription>()
                .AnyAsync(s => s.UserId == request.UserId && s.IsTrialPeriod, cancellationToken);

            if (hasUsedTrial)
            {
                return Result<CreateSubscriptionResponse>.Failure("User has already used their trial period");
            }
        }

        // Plan bilgilerini al
        var plan = SubscriptionPlan.GetPlan(request.Type);
        if (plan == null)
        {
            return Result<CreateSubscriptionResponse>.Failure($"Invalid subscription type: {request.Type}");
        }

        // Üyelik oluştur
        var subscription = Subscription.Create(
            request.UserId,
            plan,
            request.BillingCycle,
            request.StartTrial);

        // Payment method ekle
        if (!string.IsNullOrWhiteSpace(request.PaymentMethodId) && !request.StartTrial)
        {
            subscription.SetPaymentMethod(request.PaymentMethodId);
        }

        // Veritabanına ekle
        _context.Set<Subscription>().Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        // Response oluştur
        var response = new CreateSubscriptionResponse
        {
            SubscriptionId = subscription.Id, UserId = subscription.UserId, Type = subscription.Type
            , StartDate = subscription.StartDate, TrialEndDate = subscription.TrialEndDate
            , NextBillingDate = subscription.NextBillingDate, Price = subscription.Price
            , Currency = subscription.Currency, Features = subscription.Features
        };

        return Result<CreateSubscriptionResponse>.Success(response);
    }
}
