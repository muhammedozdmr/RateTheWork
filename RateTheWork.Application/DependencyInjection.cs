using System.Reflection;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RateTheWork.Application.Common.Behaviors;

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

        // AutoMapper - Extension paketi ile basit kullanım
        services.AddAutoMapper(assembly);

        return services;
    }
}
