using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Notification;

/// <summary>
/// Okunmamış bildirim spesifikasyonu
/// </summary>
public class UnreadNotificationSpecification : Specification<Entities.Notification>
{
    public override Expression<Func<Entities.Notification, bool>> ToExpression()
    {
        return notification => !notification.IsRead;
    }
}
