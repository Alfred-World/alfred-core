using System.Collections.Concurrent;

using Alfred.Core.Domain.Abstractions.Services.Ai;

using Microsoft.Extensions.Logging;

namespace Alfred.Core.Infrastructure.Providers.Ai.Common;

/// <summary>
/// Thread-safe in-memory function registry — provider-agnostic.
/// Stores factory delegates instead of instances so that scoped services (DbContext, UoW)
/// are resolved fresh from the caller's DI scope on every invocation.
/// </summary>
public sealed class AiFunctionRegistry : IAiFunctionRegistry
{
    private readonly ConcurrentDictionary<string, (AiFunctionDefinition Definition, Func<IServiceProvider, IAiFunction>
            Factory)>
        _functions = new(StringComparer.OrdinalIgnoreCase);

    private readonly ILogger<AiFunctionRegistry> _logger;

    public AiFunctionRegistry(ILogger<AiFunctionRegistry> logger)
    {
        _logger = logger;
    }

    public void RegisterFactory(
        string name,
        string description,
        object parametersSchema,
        Func<IServiceProvider, IAiFunction> factory)
    {
        var definition = new AiFunctionDefinition
        {
            Name = name,
            Description = description,
            Parameters = parametersSchema
        };

        if (_functions.TryAdd(name, (definition, factory)))
        {
            _logger.LogInformation("Registered AI function: {FunctionName}", name);
        }
        else
        {
            _logger.LogWarning("AI function '{FunctionName}' is already registered, skipping", name);
        }
    }

    public IAiFunction? GetFunction(string functionName, IServiceProvider serviceProvider)
    {
        if (!_functions.TryGetValue(functionName, out var entry))
        {
            return null;
        }

        return entry.Factory(serviceProvider);
    }

    public IReadOnlyList<AiFunctionDefinition> GetAllDefinitions()
    {
        return _functions.Values.Select(e => e.Definition).ToList();
    }
}
