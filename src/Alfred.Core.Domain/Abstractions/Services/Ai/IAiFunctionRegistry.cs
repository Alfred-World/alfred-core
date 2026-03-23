namespace Alfred.Core.Domain.Abstractions.Services.Ai;

/// <summary>
/// Registry for AI-callable functions. Functions are registered by name and can be discovered/invoked at runtime.
/// </summary>
public interface IAiFunctionRegistry
{
    /// <summary>
    /// Register a function using a factory delegate so that scoped dependencies (DbContext, UoW)
    /// are resolved fresh from the caller's DI scope on each invocation — not captured at startup.
    /// </summary>
    void RegisterFactory(
        string name,
        string description,
        object parametersSchema,
        Func<IServiceProvider, IAiFunction> factory);

    /// <summary>
    /// Resolve a function by name using the provided service provider (current request scope).
    /// Returns null if the function is not registered.
    /// </summary>
    IAiFunction? GetFunction(string functionName, IServiceProvider serviceProvider);

    /// <summary>
    /// Get all registered function definitions for inclusion in AI requests.
    /// </summary>
    IReadOnlyList<AiFunctionDefinition> GetAllDefinitions();
}

/// <summary>
/// A single AI-callable function with metadata and execution logic.
/// </summary>
public interface IAiFunction
{
    /// <summary>
    /// Unique function name (used in AI tool_call mapping).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Human-readable description for the AI to understand when to use this function.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// JSON Schema describing the function's parameters.
    /// </summary>
    object ParametersSchema { get; }

    /// <summary>
    /// Execute the function with the given JSON arguments.
    /// </summary>
    Task<AiFunctionResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an AI function invocation.
/// </summary>
public sealed class AiFunctionResult
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public object? Data { get; init; }
    public string? Error { get; init; }

    public static AiFunctionResult Success(string message, object? data = null)
    {
        return new AiFunctionResult { IsSuccess = true, Message = message, Data = data };
    }

    public static AiFunctionResult Failure(string error)
    {
        return new AiFunctionResult { IsSuccess = false, Error = error };
    }
}
