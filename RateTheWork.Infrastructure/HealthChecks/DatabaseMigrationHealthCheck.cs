using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RateTheWork.Infrastructure.Persistence;

namespace RateTheWork.Infrastructure.HealthChecks;

public class DatabaseMigrationHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseMigrationHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync
    (
        HealthCheckContext context
        , CancellationToken cancellationToken = default
    )
    {
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingMigrationsList = pendingMigrations.ToList();

            if (!pendingMigrationsList.Any())
            {
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync(cancellationToken);
                var appliedCount = appliedMigrations.Count();

                return HealthCheckResult.Healthy($"All migrations applied. Total: {appliedCount}");
            }

            return HealthCheckResult.Degraded(
                $"Database has {pendingMigrationsList.Count} pending migrations",
                data: new Dictionary<string, object>
                {
                    ["pending_migrations"] = pendingMigrationsList
                });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to check database migrations", ex);
        }
    }
}
