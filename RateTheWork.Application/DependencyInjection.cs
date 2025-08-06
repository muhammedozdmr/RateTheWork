using System.Reflection;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RateTheWork.Application.Common.Behaviors;
using RateTheWork.Application.Services.Implementations;
using RateTheWork.Application.Services.Interfaces;

namespace RateTheWork.Application;

/// <summary>
/// Application katmanı DI konfigürasyonu
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Application servislerini DI container'a ekler
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR - CQRS Pattern
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // MediatR Pipeline Behaviors (sıralama önemli!)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // AutoMapper
        services.AddAutoMapper(assembly);
        services.AddSingleton<IConfigurationProvider>(provider =>
        {
            var mapperConfig = new MapperConfiguration(cfg => { cfg.AddMaps(assembly); });
            mapperConfig.AssertConfigurationIsValid();
            return mapperConfig;
        });
        services.AddScoped<IMapper>(provider =>
            new Mapper(provider.GetRequiredService<IConfigurationProvider>(), provider.GetService));

        // Application Services
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IJobPostingService, JobPostingService>();
        services.AddScoped<ICompanySubscriptionService, CompanySubscriptionService>();
        services.AddScoped<IHRPersonnelService, HRPersonnelService>();
        services.AddScoped<IJobApplicationService, JobApplicationService>();
        services.AddScoped<IBlockchainApplicationService, BlockchainApplicationService>();
        
        // Blockchain Domain Service
        services.AddScoped<Domain.Services.BlockchainDomainService>();

        return services;
    }
}
