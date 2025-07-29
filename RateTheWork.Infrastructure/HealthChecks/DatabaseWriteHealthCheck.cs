using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RateTheWork.Infrastructure.Persistence;

namespace RateTheWork.Infrastructure.HealthChecks;

public class DatabaseWriteHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseWriteHealthCheck(ApplicationDbContext context)
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
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var testQuery = await _context.Database.ExecuteSqlRawAsync(
                "SELECT 1", cancellationToken);

            await transaction.RollbackAsync(cancellationToken);

            return HealthCheckResult.Healthy("Database read/write check passed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database write check failed: {ex.Message}", ex);
        }
    }
}
