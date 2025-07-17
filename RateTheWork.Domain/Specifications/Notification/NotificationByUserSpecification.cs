using System.Linq.Expressions;
using RateTheWork.Domain.Specifications.Common;

namespace RateTheWork.Domain.Specifications.Notification;

/// <summary>
/// Kullanıcıya göre bildirim spesifikasyonu
/// </summary>
public class NotificationByUserSpecification : Specification<Entities.Notification>
{
    private readonly string _userId;

    public NotificationByUserSpecification(string userId)
    {
        _userId = userId;
    }

    public override Expression<Func<Entities.Notification, bool>> ToExpression()
    {
        return notification => notification.UserId == _userId;
    }
}
