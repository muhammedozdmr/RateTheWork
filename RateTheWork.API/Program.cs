using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using RateTheWork.API.Filters;
using RateTheWork.Api.Middleware;
using RateTheWork.Application;
using RateTheWork.Infrastructure;
using RateTheWork.Infrastructure.Configuration;
using RateTheWork.Infrastructure.HealthChecks;
using RateTheWork.Infrastructure.Jobs;
using RateTheWork.Infrastructure.Metrics;
using RateTheWork.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Serilog yapılandırması
builder.Host.ConfigureSerilog();

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OpenTelemetry yapılandırması
builder.Services.AddOpenTelemetryConfiguration();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database-context",
        tags: new[] { "ready", "database" })
    .AddCheck<DatabaseWriteHealthCheck>(
        name: "database-write",
        tags: new[] { "ready", "database" })
    .AddCheck<DatabaseMigrationHealthCheck>(
        name: "database-migrations",
        tags: new[] { "ready", "database" })
    .AddCheck<CloudflareKVHealthCheck>(
        name: "cloudflare-kv",
        tags: new[] { "ready", "external" })
    .AddCheck<RedisHealthCheck>(
        name: "redis",
        tags: new[] { "ready", "external", "cache" });

// Add Application Layer
builder.Services.AddApplication();

// Add Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Hangfire configuration (only if not disabled)
var disableHangfire = builder.Configuration.GetValue<bool>("DisableHangfire");
if (!disableHangfire)
{
    // Hangfire server'ı başlat
    app.UseHangfireServer();

    // Background job'ları zamanla - app build edildikten sonra
    using (var scope = app.Services.CreateScope())
    {
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        
        recurringJobManager.AddOrUpdate<DataCleanupJob>(
            "cleanup-soft-deleted",
            job => job.CleanupSoftDeletedRecordsAsync(90),
            Cron.Daily(3, 0)); // Her gün saat 03:00'te

    recurringJobManager.AddOrUpdate<DataCleanupJob>(
        "cleanup-expired-verifications",
        job => job.CleanupExpiredVerificationRequestsAsync(),
        Cron.Hourly); // Her saat başı

    recurringJobManager.AddOrUpdate<DataCleanupJob>(
        "close-expired-job-postings",
        job => job.CloseExpiredJobPostingsAsync(),
        Cron.Daily(0, 0)); // Her gün gece yarısı

    recurringJobManager.AddOrUpdate<ReportGenerationJob>(
        "weekly-system-report",
        job => job.GenerateWeeklySystemReportAsync(),
        Cron.Weekly(DayOfWeek.Monday, 9, 0)); // Her pazartesi sabah 09:00'da

    // Blockchain sync jobs
    recurringJobManager.AddOrUpdate<BlockchainSyncJob>(
        "sync-pending-reviews",
        job => job.SyncPendingReviewsAsync(),
        Cron.Hourly); // Her saat başı

    recurringJobManager.AddOrUpdate<BlockchainSyncJob>(
        "create-blockchain-identities",
        job => job.CreateMissingBlockchainIdentitiesAsync(),
        Cron.Daily(2, 0)); // Her gün saat 02:00'de

    recurringJobManager.AddOrUpdate<BlockchainSyncJob>(
        "verify-blockchain-integrity",
        job => job.VerifyBlockchainIntegrityAsync(),
        Cron.Daily(4, 0)); // Her gün saat 04:00'te

    recurringJobManager.AddOrUpdate<BlockchainSyncJob>(
        "update-blockchain-statistics",
        job => job.UpdateBlockchainStatisticsAsync(),
        "*/30 * * * *"); // Her 30 dakikada bir
    }
}

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
        // Don't fail startup if migration fails - log and continue
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    // Development specific middleware can go here
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Metrics middleware
app.UseMiddleware<MetricsMiddleware>();

// Rate limiting middleware
app.UseMiddleware<RateLimitingMiddleware>();

app.UseAuthorization();

// Hangfire Dashboard (only if not disabled)
if (!disableHangfire)
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        DashboardTitle = "RateTheWork Background Jobs", 
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}

app.MapControllers();

// Health check endpoints with detailed responses
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"), ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false, ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/database", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database"), ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/external", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("external"), ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

// Root endpoint
app.MapGet("/", () => Results.Ok(new
{
    service = "RateTheWork API", version = "1.0.0", status = "running", documentation = "/swagger", health = new
    {
        all = "/health", ready = "/health/ready", live = "/health/live", database = "/health/database"
        , external = "/health/external"
    }
}));

app.Run();
