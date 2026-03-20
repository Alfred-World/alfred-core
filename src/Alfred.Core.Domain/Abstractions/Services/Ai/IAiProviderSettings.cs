namespace Alfred.Core.Domain.Abstractions.Services.Ai;

/// <summary>
/// Read-only AI provider configuration exposed to the Application layer.
/// Implemented in Infrastructure by GroqOptions (or other provider options).
/// </summary>
public interface IAiProviderSettings
{
    /// <summary>
    /// Primary default model for text/function-calling tasks.
    /// When multiple models are configured, this returns the first one.
    /// </summary>
    string DefaultModel => DefaultModels.FirstOrDefault() ?? "unknown";

    /// <summary>
    /// Primary model for vision (image analysis) tasks.
    /// When multiple models are configured, this returns the first one.
    /// </summary>
    string VisionModel => VisionModels.FirstOrDefault() ?? "unknown";

    /// <summary>
    /// List of models for text/function-calling tasks.
    /// The system rotates through these when one exhausts its token quota.
    /// </summary>
    IReadOnlyList<string> DefaultModels { get; }

    /// <summary>
    /// List of models for vision (image analysis) tasks.
    /// The system rotates through these when one exhausts its token quota.
    /// </summary>
    IReadOnlyList<string> VisionModels { get; }
}
