using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateTheWork.Infrastructure.Persistence;
using RateTheWork.Infrastructure.Persistence.Repositories;
using RateTheWork.Infrastructure.Services;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Interfaces.Repositories;

namespace RateTheWork.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database - Try both DATABASE_URL and ConnectionStrings:DefaultConnection
        var connectionString = configuration["DATABASE_URL"] 
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? throw new InvalidOperationException("DATABASE_URL not configured");
            
        // Convert Railway DATABASE_URL to Npgsql format if it's a URL
        string npgsqlConnectionString;
        if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
        {
            // Parse URL format: postgresql://user:password@host:port/database
            var uri = new Uri(connectionString.Replace("postgres://", "postgresql://"));
            var userInfo = uri.UserInfo.Split(':');
            
            npgsqlConnectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        }
        else
        {
            // Already in standard format
            npgsqlConnectionString = connectionString;
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(npgsqlConnectionString));

        // Secret Management
        services.AddSingleton<ISecretService, CloudflareKVService>();
        
        // Email Service
        // TODO: Implement SendGridEmailService
        // services.AddScoped<IEmailService, SendGridEmailService>();
        
        // SMS Service  
        // TODO: Implement TwilioSmsService
        // services.AddScoped<IPushNotificationService, TwilioSmsService>();
        
        // Repositories
        services.AddScoped<Application.Common.Interfaces.IUnitOfWork, UnitOfWork>();
        services.AddScoped<Domain.Interfaces.Repositories.IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}