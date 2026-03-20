using Alfred.Core.Domain.Abstractions.Services.Ai;
using Alfred.Core.Infrastructure.Providers.Ai.Common;

using Microsoft.Extensions.DependencyInjection;

namespace Alfred.Core.Infrastructure.Providers.Ai.Groq;

/// <summary>
/// Extension methods for registering the Groq AI provider.
/// To switch providers in the future, replace this call in InfrastructureModule
/// with a different provider extension (e.g., services.AddOpenAiProvider()).
/// </summary>
public static class GroqAiProviderExtensions
{
    /// <summary>
    /// Register the Groq AI provider with automatic API key and model rotation.
    /// Reads configuration from environment variables:
    /// - GROQ_API_KEYS: comma-separated list of API keys (required)
    /// - GROQ_DEFAULT_MODELS: comma-separated list of text/function-calling models (default: llama-3.3-70b-versatile)
    /// - GROQ_VISION_MODELS: comma-separated list of vision models (default: llama-3.2-90b-vision-preview)
    /// - GROQ_BASE_URL: API base URL (default: https://api.groq.com/openai/v1)
    /// - GROQ_KEY_COOLDOWN_MINUTES: cooldown after key rate limit (default: 60)
    /// - GROQ_MODEL_COOLDOWN_MINUTES: cooldown after model token exhaustion (default: 60)
    /// - GROQ_MAX_RETRIES: max retry attempts across keys+models (default: 3)
    /// - GROQ_TIMEOUT_SECONDS: HTTP request timeout (default: 30)
    /// </summary>
    public static IServiceCollection AddGroqAiProvider(this IServiceCollection services)
    {
        // Parse API keys
        var apiKeysRaw = Environment.GetEnvironmentVariable("GROQ_API_KEYS") ?? "";
        var apiKeys = apiKeysRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToArray();

        // Parse default models (with backward comp for single GROQ_DEFAULT_MODEL)
        var defaultModelsRaw = Environment.GetEnvironmentVariable("GROQ_DEFAULT_MODELS")
            ?? Environment.GetEnvironmentVariable("GROQ_DEFAULT_MODEL")
            ?? "";
        var defaultModels = defaultModelsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToArray();

        // Parse vision models (with backward comp for single GROQ_VISION_MODEL)
        var visionModelsRaw = Environment.GetEnvironmentVariable("GROQ_VISION_MODELS")
            ?? Environment.GetEnvironmentVariable("GROQ_VISION_MODEL")
            ?? "";
        var visionModels = visionModelsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToArray();

        var options = new GroqOptions
        {
            ApiKeys = apiKeys,
            DefaultModels = defaultModels,
            VisionModels = visionModels
        };

        var baseUrl = Environment.GetEnvironmentVariable("GROQ_BASE_URL");
        if (!string.IsNullOrWhiteSpace(baseUrl))
            options.BaseUrl = baseUrl;

        if (int.TryParse(Environment.GetEnvironmentVariable("GROQ_KEY_COOLDOWN_MINUTES"), out var keyCooldown))
            options.KeyCooldownMinutes = keyCooldown;

        if (int.TryParse(Environment.GetEnvironmentVariable("GROQ_MODEL_COOLDOWN_MINUTES"), out var modelCooldown))
            options.ModelCooldownMinutes = modelCooldown;

        if (int.TryParse(Environment.GetEnvironmentVariable("GROQ_MAX_RETRIES"), out var maxRetries))
            options.MaxRetries = maxRetries;

        if (int.TryParse(Environment.GetEnvironmentVariable("GROQ_TIMEOUT_SECONDS"), out var timeout))
            options.TimeoutSeconds = timeout;

        options.Validate();

        // Options — singleton (concrete type + interface)
        services.AddSingleton(options);
        services.AddSingleton<IAiProviderSettings>(options);

        // Key rotation manager — singleton (shared state across requests)
        services.AddSingleton<IAiKeyRotationManager, GroqKeyRotationManager>();

        // Model rotation manager — singleton (tracks model exhaustion)
        services.AddSingleton<GroqModelRotationManager>();

        // Groq HTTP client — typed client for proper lifecycle management
        services.AddHttpClient<GroqAiClient>();
        services.AddScoped<IAiClient, GroqAiClient>();

        // Function registry — singleton (registered once at startup)
        services.AddSingleton<IAiFunctionRegistry, AiFunctionRegistry>();

        return services;
    }
}
