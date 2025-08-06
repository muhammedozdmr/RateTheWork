using System.Net;
using System.Text.Json;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Api.Middleware;

/// <summary>
/// Rate limiting middleware
/// </summary>
public class RateLimitingMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public RateLimitingMiddleware
    (
        RequestDelegate next
        , IServiceProvider serviceProvider
        , ILogger<RateLimitingMiddleware> logger
        , IConfiguration configuration
    )
    {
        _next = next;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Health check endpoint'lerini hariç tut
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var rateLimitingService = scope.ServiceProvider.GetRequiredService<IRateLimitingService>();
        var currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();

        var ipAddress = GetIpAddress(context);
        var endpoint = context.Request.Path.Value ?? "unknown";
        var method = context.Request.Method;

        // IP bazlı rate limiting
        var ipLimit = _configuration.GetValue<int>("RateLimiting:PerIpPerMinute", 60);
        var ipResult = await rateLimitingService.CheckIpRateLimitAsync(
            ipAddress,
            ipLimit,
            TimeSpan.FromMinutes(1));

        if (!ipResult.IsAllowed)
        {
            await WriteRateLimitResponse(context, ipResult);
            return;
        }

        // Kullanıcı bazlı rate limiting (giriş yapmış kullanıcılar için)
        var userId = currentUserService.UserId;
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
        {
            var userLimit = _configuration.GetValue<int>("RateLimiting:PerUserPerMinute", 100);
            var userResult = await rateLimitingService.CheckUserRateLimitAsync(
                userGuid,
                "api",
                userLimit,
                TimeSpan.FromMinutes(1));

            if (!userResult.IsAllowed)
            {
                await WriteRateLimitResponse(context, userResult);
                return;
            }

            // Response header'larına rate limit bilgilerini ekle
            AddRateLimitHeaders(context, userResult);
        }
        else
        {
            // Giriş yapmamış kullanıcılar için IP bazlı header'lar
            AddRateLimitHeaders(context, ipResult);
        }

        // Özel endpoint'ler için ek rate limiting
        if (endpoint.Contains("/login", StringComparison.OrdinalIgnoreCase))
        {
            var loginLimit = _configuration.GetValue<int>("RateLimiting:LoginAttemptsPerHour", 5);
            var loginResult = await rateLimitingService.CheckRateLimitAsync(
                $"login:{ipAddress}",
                loginLimit,
                TimeSpan.FromHours(1));

            if (!loginResult.IsAllowed)
            {
                _logger.LogWarning("Çok fazla giriş denemesi: {IpAddress}", ipAddress);
                await WriteRateLimitResponse(context, loginResult
                    , "Çok fazla giriş denemesi. Lütfen daha sonra tekrar deneyin.");
                return;
            }
        }
        else if (endpoint.Contains("/password-reset", StringComparison.OrdinalIgnoreCase))
        {
            var resetLimit = _configuration.GetValue<int>("RateLimiting:PasswordResetPerDay", 3);
            var resetResult = await rateLimitingService.CheckRateLimitAsync(
                $"password-reset:{ipAddress}",
                resetLimit,
                TimeSpan.FromDays(1));

            if (!resetResult.IsAllowed)
            {
                _logger.LogWarning("Çok fazla şifre sıfırlama talebi: {IpAddress}", ipAddress);
                await WriteRateLimitResponse(context, resetResult
                    , "Çok fazla şifre sıfırlama talebi. Lütfen 24 saat sonra tekrar deneyin.");
                return;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// IP adresini alır
    /// </summary>
    private string GetIpAddress(HttpContext context)
    {
        // Cloudflare veya diğer proxy'lerden gelen IP'yi kontrol et
        if (context.Request.Headers.ContainsKey("CF-Connecting-IP"))
        {
            return context.Request.Headers["CF-Connecting-IP"].ToString();
        }

        if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            return forwardedFor.Split(',')[0].Trim();
        }

        if (context.Request.Headers.ContainsKey("X-Real-IP"))
        {
            return context.Request.Headers["X-Real-IP"].ToString();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Rate limit header'larını ekler
    /// </summary>
    private void AddRateLimitHeaders(HttpContext context, RateLimitResult result)
    {
        context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = result.ResetAt.ToUnixTimeSeconds().ToString();
    }

    /// <summary>
    /// Rate limit aşıldığında response yazar
    /// </summary>
    private async Task WriteRateLimitResponse(HttpContext context, RateLimitResult result, string? customMessage = null)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.Headers["Retry-After"] =
            ((int)(result.ResetAt - DateTimeOffset.UtcNow).TotalSeconds).ToString();

        AddRateLimitHeaders(context, result);

        var response = new
        {
            error = "Rate limit exceeded", message = customMessage ?? "Too many requests. Please try again later."
            , retryAfter = result.ResetAt
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
