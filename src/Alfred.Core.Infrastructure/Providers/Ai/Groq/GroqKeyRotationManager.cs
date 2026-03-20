using System.Collections.Concurrent;

using Alfred.Core.Domain.Abstractions.Services.Ai;

using Microsoft.Extensions.Logging;

namespace Alfred.Core.Infrastructure.Providers.Ai.Groq;

/// <summary>
/// Thread-safe API key rotation manager for Groq free-tier accounts.
/// Rotates through multiple keys using round-robin and automatically
/// skips keys that have been marked as rate-limited.
/// </summary>
public sealed class GroqKeyRotationManager : IAiKeyRotationManager
{
    private readonly string[] _apiKeys;
    private readonly ConcurrentDictionary<string, KeyState> _keyStates;
    private readonly ILogger<GroqKeyRotationManager> _logger;
    private int _currentIndex;

    public GroqKeyRotationManager(GroqOptions options, ILogger<GroqKeyRotationManager> logger)
    {
        _apiKeys = options.ApiKeys;
        _logger = logger;
        _keyStates = new ConcurrentDictionary<string, KeyState>();

        foreach (var key in _apiKeys)
        {
            _keyStates[key] = new KeyState();
        }
    }

    public string? GetNextKey()
    {
        if (_apiKeys.Length == 0)
            return null;

        var totalKeys = _apiKeys.Length;

        // Try each key once via round-robin
        for (var attempt = 0; attempt < totalKeys; attempt++)
        {
            var index = Interlocked.Increment(ref _currentIndex) % totalKeys;
            // Handle negative modulo edge case after int overflow
            if (index < 0) index += totalKeys;

            var key = _apiKeys[index];
            var state = _keyStates.GetOrAdd(key, _ => new KeyState());

            if (state.IsAvailable)
            {
                _logger.LogDebug("Using API key ...{KeySuffix} (slot {Index})",
                    GetKeySuffix(key), index);
                return key;
            }

            // Check if cooldown expired
            if (state.CooldownUntil.HasValue && DateTime.UtcNow >= state.CooldownUntil.Value)
            {
                state.Reset();
                _logger.LogInformation("API key ...{KeySuffix} cooldown expired, re-enabling",
                    GetKeySuffix(key));
                return key;
            }
        }

        _logger.LogWarning("All {Count} API keys are exhausted or cooling down", totalKeys);
        return null;
    }

    public void MarkKeyExhausted(string apiKey, TimeSpan cooldownDuration)
    {
        var state = _keyStates.GetOrAdd(apiKey, _ => new KeyState());
        state.IsAvailable = false;
        state.CooldownUntil = DateTime.UtcNow.Add(cooldownDuration);
        state.FailureCount++;

        _logger.LogWarning(
            "API key ...{KeySuffix} marked as exhausted. Cooldown until {CooldownUntil}. Failures: {Failures}",
            GetKeySuffix(apiKey), state.CooldownUntil, state.FailureCount);
    }

    public void MarkKeySuccess(string apiKey)
    {
        var state = _keyStates.GetOrAdd(apiKey, _ => new KeyState());
        state.SuccessCount++;
    }

    public IReadOnlyList<AiKeyStatus> GetKeyStatuses()
    {
        return _apiKeys.Select(key =>
        {
            var state = _keyStates.GetOrAdd(key, _ => new KeyState());
            var isAvailable = state.IsAvailable ||
                              (state.CooldownUntil.HasValue && DateTime.UtcNow >= state.CooldownUntil.Value);

            return new AiKeyStatus
            {
                KeySuffix = GetKeySuffix(key),
                IsAvailable = isAvailable,
                CooldownUntil = state.CooldownUntil,
                SuccessCount = state.SuccessCount,
                FailureCount = state.FailureCount
            };
        }).ToList();
    }

    private static string GetKeySuffix(string key)
    {
        return key.Length > 8 ? key[^8..] : "****";
    }

    private sealed class KeyState
    {
        public bool IsAvailable { get; set; } = true;
        public DateTime? CooldownUntil { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }

        public void Reset()
        {
            IsAvailable = true;
            CooldownUntil = null;
        }
    }
}
