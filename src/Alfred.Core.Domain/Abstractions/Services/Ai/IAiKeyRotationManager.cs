namespace Alfred.Core.Domain.Abstractions.Services.Ai;

/// <summary>
/// Manages multiple API keys for an AI provider, supporting round-robin rotation
/// and automatic failover when a key hits its rate limit or token quota.
/// </summary>
public interface IAiKeyRotationManager
{
    /// <summary>
    /// Get the next available API key. Automatically skips exhausted keys.
    /// </summary>
    /// <returns>An active API key, or null if all keys are exhausted.</returns>
    string? GetNextKey();

    /// <summary>
    /// Mark a key as rate-limited with an optional cooldown period.
    /// The key will be skipped until the cooldown expires.
    /// </summary>
    void MarkKeyExhausted(string apiKey, TimeSpan cooldownDuration);

    /// <summary>
    /// Report a successful usage of a key (resets failure counters if any).
    /// </summary>
    void MarkKeySuccess(string apiKey);

    /// <summary>
    /// Get a diagnostic summary of all key statuses.
    /// </summary>
    IReadOnlyList<AiKeyStatus> GetKeyStatuses();
}

/// <summary>
/// Status information for a single API key slot.
/// </summary>
public sealed class AiKeyStatus
{
    public required string KeySuffix { get; init; }
    public required bool IsAvailable { get; init; }
    public DateTime? CooldownUntil { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
}
