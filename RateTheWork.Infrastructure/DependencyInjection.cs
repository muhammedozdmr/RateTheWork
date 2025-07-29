using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateTheWork.Infrastructure.Persistence;
using RateTheWork.Infrastructure.Services;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration["DATABASE_URL"] ?? 
            throw new InvalidOperationException("DATABASE_URL not configured");
            
        // Convert Railway DATABASE_URL to Npgsql format
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString)
        {
            SslMode = Npgsql.SslMode.Require,
            TrustServerCertificate = true
        };

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.ConnectionString));

        // Secret Management
        services.AddSingleton<ISecretService, CloudflareKVService>();
        
        // Email Service
        services.AddScoped<IEmailService, SendGridEmailService>();
        
        // SMS Service  
        services.AddScoped<IPushNotificationService, TwilioSmsService>();
        
        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IReviewVoteRepository, ReviewVoteRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IJobPostingRepository, JobPostingRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IContractorReviewRepository, ContractorReviewRepository>();
        services.AddScoped<ICVApplicationRepository, CVApplicationRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }
}