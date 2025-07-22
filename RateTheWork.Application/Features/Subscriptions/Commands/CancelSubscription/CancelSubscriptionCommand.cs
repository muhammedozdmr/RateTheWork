using MediatR;
using RateTheWork.Application.Common.Models;

namespace RateTheWork.Application.Features.Subscriptions.Commands.CancelSubscription;

/// <summary>
/// Üyelik iptal etme komutu
/// </summary>
public record CancelSubscriptionCommand : IRequest<Result>
{
    public string SubscriptionId { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public bool CancelImmediately { get; init; }
}
