using Microsoft.Extensions.Diagnostics.HealthChecks;
using RateTheWork.Infrastructure.Services;

namespace RateTheWork.Infrastructure.HealthChecks;

public class CloudflareKVHealthCheck : IHealthCheck
{
    private const string TestKey = "health_check_test";
    private readonly ISecretService _secretService;

    public CloudflareKVHealthCheck(ISecretService secretService)
    {
        _secretService = secretService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync
    (
        HealthCheckContext context
        , CancellationToken cancellationToken = default
    )
    {
        try
        {
            var testValue = $"health_check_{DateTime.UtcNow.Ticks}";

            await _secretService.SetSecretAsync(TestKey, testValue);

            var retrievedValue = await _secretService.GetSecretAsync(TestKey);

            if (retrievedValue == testValue)
            {
                await _secretService.DeleteSecretAsync(TestKey);
                return HealthCheckResult.Healthy("Cloudflare KV is working properly");
            }

            return HealthCheckResult.Unhealthy("Cloudflare KV read/write test failed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Cloudflare KV check failed: {ex.Message}", ex);
        }
    }
}
