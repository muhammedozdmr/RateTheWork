using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace RateTheWork.Infrastructure.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private IConnectionMultiplexer? _connection;

    public RedisHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync
    (
        HealthCheckContext context
        , CancellationToken cancellationToken = default
    )
    {
        try
        {
            var connectionString = _configuration["REDIS_CONNECTION_STRING"] ??
                                   _configuration.GetConnectionString("Redis");

            if (string.IsNullOrEmpty(connectionString))
            {
                return HealthCheckResult.Healthy("Redis not configured");
            }

            _connection ??= await ConnectionMultiplexer.ConnectAsync(connectionString);

            var database = _connection.GetDatabase();
            var key = "health_check_test";
            var value = DateTime.UtcNow.Ticks.ToString();

            await database.StringSetAsync(key, value, TimeSpan.FromSeconds(30));
            var retrieved = await database.StringGetAsync(key);

            if (retrieved == value)
            {
                await database.KeyDeleteAsync(key);

                var endpoints = _connection.GetEndPoints();
                var server = _connection.GetServer(endpoints.First());
                var info = await server.InfoAsync();

                return HealthCheckResult.Healthy($"Redis is working. Server: {endpoints.First()}");
            }

            return HealthCheckResult.Unhealthy("Redis read/write test failed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Redis check failed: {ex.Message}", ex);
        }
    }
}
