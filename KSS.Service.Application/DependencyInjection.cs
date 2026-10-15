using FluentValidation;
using KSS.Common.Behaviors;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System.Reflection;

namespace KSS.Service.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add Mapster
        services.AddMapster();

        // Compile mappings for validation and performance
        try
        {
            TypeAdapterConfig.GlobalSettings.Compile();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Mapster compilation warning: {ex.Message}");
        }

        // Assembly reference
        var assembly = Assembly.GetExecutingAssembly();

        // Add MediatR with pipeline behaviors
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            // Add pipeline behaviors in order (execution order matters)
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviorApi<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviorApi<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviorApi<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehaviorApi<,>));
        });

        // Add FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Add Feature Management for feature flags
        services.AddFeatureManagement();

        return services;
    }

    private static IServiceCollection AddMapster(this IServiceCollection services)
    {
        // Register Mapster configuration and scan for mappings
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(ApplicationAssemblyMarker).Assembly);

        // Register TypeAdapterConfig as singleton for dependency injection if needed
        services.AddSingleton(config);

        return services;
    }
}
