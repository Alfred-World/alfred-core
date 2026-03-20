using Alfred.Core.Domain.Abstractions.Services.Ai;

namespace Alfred.Core.Infrastructure.Providers.Ai.Groq;

/// <summary>
/// Configuration options for the Groq AI provider.
/// Supports multiple API keys and models for rotation on free-tier accounts.
/// </summary>
public sealed class GroqOptions : IAiProviderSettings
{
    /// <summary>
    /// Base URL for the Groq API. Defaults to the official endpoint.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";

    /// <summary>
    /// Comma-separated list of API keys. The system rotates through these
    /// when one key hits its rate/token limit.
    /// </summary>
    public string[] ApiKeys { get; set; } = [];

    /// <summary>
    /// Comma-separated list of models for text/function-calling tasks.
    /// When a model exhausts its token quota, the system automatically tries the next model.
    /// </summary>
    public string[] DefaultModels { get; set; } = ["llama-3.3-70b-versatile"];

    /// <summary>
    /// Comma-separated list of models for vision (image analysis) tasks.
    /// When a model exhausts its token quota, the system automatically tries the next model.
    /// </summary>
    public string[] VisionModels { get; set; } = ["llama-3.2-90b-vision-preview"];

    /// <summary>
    /// How long (in minutes) to cool down a rate-limited key before retrying.
    /// </summary>
    public int KeyCooldownMinutes { get; set; } = 60;

    /// <summary>
    /// How long (in minutes) to cool down an exhausted model before retrying.
    /// </summary>
    public int ModelCooldownMinutes { get; set; } = 60;

    /// <summary>
    /// Maximum number of retries with different keys+models before giving up.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    // Interface implementations for backward compatibility
    IReadOnlyList<string> IAiProviderSettings.DefaultModels => DefaultModels;
    IReadOnlyList<string> IAiProviderSettings.VisionModels => VisionModels;

    public void Validate()
    {
        if (ApiKeys.Length == 0)
        {
            throw new InvalidOperationException(
                "At least one GROQ_API_KEYS must be configured. " +
                "Set GROQ_API_KEYS environment variable with comma-separated keys.");
        }

        if (DefaultModels.Length == 0)
        {
            throw new InvalidOperationException(
                "At least one GROQ_DEFAULT_MODELS must be configured. " +
                "Set GROQ_DEFAULT_MODELS environment variable with comma-separated models.");
        }

        if (VisionModels.Length == 0)
        {
            throw new InvalidOperationException(
                "At least one GROQ_VISION_MODELS must be configured. " +
                "Set GROQ_VISION_MODELS environment variable with comma-separated models.");
        }

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new InvalidOperationException("GROQ_BASE_URL cannot be empty.");
        }
    }
}
