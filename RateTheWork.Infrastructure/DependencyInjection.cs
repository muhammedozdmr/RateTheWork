using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Interfaces.Repositories;
using RateTheWork.Infrastructure.Cache;
using RateTheWork.Infrastructure.Configuration;
using RateTheWork.Infrastructure.HealthChecks;
using RateTheWork.Infrastructure.Jobs;
using RateTheWork.Infrastructure.Persistence;
using RateTheWork.Infrastructure.Persistence.Interceptors;
using RateTheWork.Infrastructure.Persistence.Repositories;
using RateTheWork.Infrastructure.Services;
using IUnitOfWork = RateTheWork.Application.Common.Interfaces.IUnitOfWork;

namespace RateTheWork.Infrastructure;

/// <summary>
/// Infrastructure katmanı için dependency injection yapılandırmaları
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Yapılandırma Seçenekleri
        services.Configure<FirebaseOptions>(configuration.GetSection("Firebase"));
        services.Configure<CloudflareOptions>(configuration.GetSection(CloudflareOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<SmsOptions>(configuration.GetSection(SmsOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
        // Veritabanı - Hem DATABASE_URL hem de ConnectionStrings:DefaultConnection dene
        var connectionString = configuration["DATABASE_URL"]
                               ?? configuration.GetConnectionString("DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                               ?? throw new InvalidOperationException("DATABASE_URL not configured");

        // Eğer URL ise Railway DATABASE_URL'yi Npgsql formatına dönüştür
        string npgsqlConnectionString;
        if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
        {
            // URL formatını ayrıştır: postgresql://kullanıcı:şifre@sunucu:port/veritabanı
            var uri = new Uri(connectionString.Replace("postgres://", "postgresql://"));
            var userInfo = uri.UserInfo.Split(':');

            npgsqlConnectionString =
                $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        }
        else
        {
            // Zaten standart formatta
            npgsqlConnectionString = connectionString;
        }

        // Veritabanı Yakalayıcıları
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<PerformanceInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(npgsqlConnectionString);

            // Yakalayıcıları ekle
            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
                serviceProvider.GetRequiredService<SoftDeleteInterceptor>(),
                serviceProvider.GetRequiredService<PerformanceInterceptor>()
            );
        });

        // Gizli Bilgi Yönetimi
        services.AddSingleton<ISecretService, CloudflareKVService>();

        // E-posta Servisi
        services.AddScoped<IEmailService, SendGridEmailService>();

        // SMS Servisi  
        services.AddScoped<ISmsService, TwilioSmsService>();

        // Anlık Bildirim Servisi
        services.AddScoped<IPushNotificationService, FirebasePushNotificationService>();

        // Önbellek Servisi
        services.AddMemoryCache();

        var useRedis = configuration.GetValue<bool>("Cache:UseRedis");
        if (useRedis && !string.IsNullOrEmpty(configuration["REDIS_CONNECTION_STRING"]))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["REDIS_CONNECTION_STRING"];
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }

        // Dosya Depolama Servisi - Yerel Depolama (Railway'de volume mount kullanabilirsiniz)
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Depolar
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<Domain.Interfaces.Repositories.IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICompanyBranchRepository, CompanyBranchRepository>();
        services.AddScoped<IJobPostingRepository, JobPostingRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IBadgeRepository, BadgeRepository>();
        services.AddScoped<IContractorReviewRepository, ContractorReviewRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReviewVoteRepository, ReviewVoteRepository>();
        services.AddScoped<IVerificationRequestRepository, VerificationRequestRepository>();
        services.AddScoped<ICVApplicationRepository, CVApplicationRepository>();

        // Sağlık Kontrolleri
        services.AddScoped<CloudflareKVHealthCheck>();
        services.AddScoped<DatabaseMigrationHealthCheck>();
        services.AddScoped<DatabaseWriteHealthCheck>();
        services.AddScoped<RedisHealthCheck>();

        // Güvenlik Servisleri
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITcIdentityValidationService, TcIdentityValidationService>();
        services.AddScoped<IDateTimeService, DateTimeService>();

        // HTTP Bağlamı
        services.AddHttpContextAccessor();
        services.AddHttpClient();

        // Arka Plan İşleri (Hangfire)
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => { options.UseNpgsqlConnection(npgsqlConnectionString); }));

        services.AddHangfireServer(options =>
        {
            options.ServerName = $"RateTheWork-{Environment.MachineName}";
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "critical", "default", "low" };
        });

        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        // Dağıtık Servisler
        services.AddSingleton<IDistributedLockService, DistributedLockService>();
        services.AddSingleton<IFeatureFlagService, FeatureFlagService>();

        // Distributed Cache (Redis veya In-Memory)
        services.AddDistributedMemoryCache(); // Fallback for IDistributedCache
        services.AddSingleton<IMetricsService, MetricsService>();
        services.AddScoped<IRateLimitingService, RateLimitingService>();

        // Hangfire Job Scheduler
        services.AddHostedService<JobScheduler>();

        // Cache Invalidator
        services.AddScoped<CacheInvalidator>();

        return services;
    }
}
