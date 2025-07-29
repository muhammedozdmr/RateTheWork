using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Common;

namespace RateTheWork.Infrastructure.Persistence.Interceptors;

public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public SoftDeleteInterceptor
    (
        IDateTimeService dateTimeService
        , ICurrentUserService currentUserService
    )
    {
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateSoftDeleteEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync
        (DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateSoftDeleteEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateSoftDeleteEntities(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker
            .Entries<ISoftDelete>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;

            // Eğer varlık AuditableBaseEntity ise, SoftDelete metodunu kullan
            if (entry.Entity is AuditableBaseEntity auditableEntity)
            {
                auditableEntity.SoftDelete(_currentUserService.UserId ?? "System");
            }
        }
    }
}
