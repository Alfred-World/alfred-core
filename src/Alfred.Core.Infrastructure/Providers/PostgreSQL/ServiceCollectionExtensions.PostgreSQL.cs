using System.Reflection;

using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Abstractions.Services;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Common.Options;
using Alfred.Core.Infrastructure.Common.Seeding;
using Alfred.Core.Infrastructure.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// Extension methods for configuring PostgreSQL provider
/// Uses convention-based auto-registration to minimize boilerplate
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly Assembly DomainAssembly = typeof(IRepository<>).Assembly;
    private static readonly Assembly InfraAssembly = typeof(ServiceCollectionExtensions).Assembly;

    public static IServiceCollection AddPostgreSQL(this IServiceCollection services, string connectionString)
    {
        PostgreSqlOptions options = new() { ConnectionString = connectionString };
        return AddPostgreSQL(services, options);
    }

    public static IServiceCollection AddPostgreSQL(this IServiceCollection services, PostgreSqlOptions options)
    {
        // === DbContext & Unit of Work ===
        services.AddScoped<IDbContextFactory, PostgreSqlDbContextFactory>(_ => new PostgreSqlDbContextFactory(options));
        services.AddScoped<PostgreSqlDbContext>(_ => new PostgreSqlDbContext(options));
        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<PostgreSqlDbContext>());
        services.AddScoped<IUnitOfWork, DefaultUnitOfWork>();

        // === Auto-register Repositories (IXxxRepository -> XxxRepository) ===
        services.AddByConvention(
            DomainAssembly,
            InfraAssembly,
            "Alfred.Core.Domain.Abstractions.Repositories",
            "Alfred.Core.Infrastructure.Repositories"
        );

        // Services in Domain.Abstractions.Services -> Infrastructure.Services
        services.AddByConvention(
            DomainAssembly,
            InfraAssembly,
            "Alfred.Core.Domain.Abstractions.Services",
            "Alfred.Core.Infrastructure.Services"
        );

        // === HttpClient Services (special registration) ===
        services.AddHttpClient<ILocationService, IpApiLocationService>();

        // === Auto-register Data Seeders ===
        services.AddImplementationsOf<IDataSeeder>(InfraAssembly);

        return services;
    }

    /// <summary>
    /// Auto-register implementations matching interface naming convention (IXxx -> Xxx)
    /// </summary>
    private static void AddByConvention(
        this IServiceCollection services,
        Assembly interfaceAssembly,
        Assembly implementationAssembly,
        string interfaceNamespace,
        string implementationNamespace)
    {
        var interfaces = interfaceAssembly.GetTypes()
            .Where(t => t.IsInterface && t.Namespace == interfaceNamespace);

        foreach (var iface in interfaces)
        {
            // Convention: IUserRepository -> UserRepository
            var implName = iface.Name.StartsWith("I") ? iface.Name[1..] : iface.Name;
            var implType = implementationAssembly.GetTypes()
                .FirstOrDefault(t =>
                    t.Name == implName && t.Namespace == implementationNamespace && iface.IsAssignableFrom(t));

            if (implType != null)
            {
                services.AddScoped(iface, implType);
            }
        }
    }

    /// <summary>
    /// Auto-register all implementations of a given interface type
    /// </summary>
    private static void AddImplementationsOf<TInterface>(this IServiceCollection services, Assembly assembly)
    {
        var implementations = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(TInterface).IsAssignableFrom(t));

        foreach (var impl in implementations)
        {
            services.AddScoped(typeof(TInterface), impl);
        }
    }
}
