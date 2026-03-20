using Alfred.Core.Infrastructure.Common.Extensions;
using Alfred.Core.Infrastructure.Providers.Ai.Groq;

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

        // AI provider — swap this line to change providers (e.g. AddOpenAiProvider())
        services.AddGroqAiProvider();

        return services;
    }
}
