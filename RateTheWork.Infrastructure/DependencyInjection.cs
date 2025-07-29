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
        // Database
        var connectionString = configuration["DATABASE_URL"] ?? 
            throw new InvalidOperationException("DATABASE_URL not configured");
            
        // Convert Railway DATABASE_URL to Npgsql format
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString)
        {
            SslMode = Npgsql.SslMode.Require
        };

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.ConnectionString));

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