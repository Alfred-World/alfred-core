using Alfred.Core.Infrastructure.Common.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace Alfred.Core.Infrastructure;

/// <summary>
/// Infrastructure module - configures the appropriate database provider and services
/// </summary>
public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDatabase();
        services.AddRepositories();
        services.AddInfrastructureServices();

        return services;
    }
}
