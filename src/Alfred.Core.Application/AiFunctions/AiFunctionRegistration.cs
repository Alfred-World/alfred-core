using Alfred.Core.Application.AiFunctions.Functions;
using Alfred.Core.Domain.Abstractions.Services.Ai;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alfred.Core.Application.AiFunctions;

/// <summary>
/// Registers all AI-callable functions into the function registry at application startup.
/// Uses a short-lived probe scope only to read function metadata (Name, Description, ParametersSchema).
/// The actual factory stored in the registry resolves a fresh instance per request scope,
/// ensuring scoped dependencies (IUnitOfWork, DbContext) are never captured at startup.
/// </summary>
public static class AiFunctionRegistration
{
    public static void RegisterAiFunctions(this IServiceProvider serviceProvider)
    {
        var registry = serviceProvider.GetRequiredService<IAiFunctionRegistry>();
        var logger = serviceProvider.GetRequiredService<ILogger<AiFunctionCallService>>();

        // Probe scope: only used to read metadata from function properties (no DB access occurs here).
        // Each RegisterFunction call stores a factory — not the probe instance — in the registry.
        using var probeScope = serviceProvider.CreateScope();
        var probeSp = probeScope.ServiceProvider;

        RegisterFunction<CreateBrandsFunction>(registry, probeSp);
        RegisterFunction<CreateCategoriesFunction>(registry, probeSp);

        logger.LogInformation("Registered {Count} AI functions", registry.GetAllDefinitions().Count);
    }

    /// <summary>
    /// Reads metadata from a probe instance (safe — no DB access in Name/Description/ParametersSchema),
    /// then stores a factory that resolves a fresh scoped instance per request.
    /// </summary>
    private static void RegisterFunction<T>(IAiFunctionRegistry registry, IServiceProvider probeSp)
        where T : class, IAiFunction
    {
        var probe = probeSp.GetRequiredService<T>();
        registry.RegisterFactory(
            probe.Name,
            probe.Description,
            probe.ParametersSchema,
            sp => sp.GetRequiredService<T>());
    }
}
