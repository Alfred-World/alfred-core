using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace Alfred.Core.Infrastructure.Providers.Ai.Groq;

/// <summary>
/// Thread-safe model rotation manager for Groq.
/// Rotates through multiple models when one exhausts its token quota.
/// </summary>
public sealed class GroqModelRotationManager
{
    private readonly string[] _defaultModels;
    private readonly string[] _visionModels;
    private readonly ConcurrentDictionary<string, ModelState> _modelStates;
    private readonly ILogger<GroqModelRotationManager> _logger;
    private int _defaultModelIndex;
    private int _visionModelIndex;

    public GroqModelRotationManager(GroqOptions options, ILogger<GroqModelRotationManager> logger)
    {
        _defaultModels = options.DefaultModels;
        _visionModels = options.VisionModels;
        _logger = logger;
        _modelStates = new ConcurrentDictionary<string, ModelState>();

        // Initialize all models
        foreach (var model in _defaultModels.Concat(_visionModels).Distinct())
        {
            _modelStates[model] = new ModelState();
        }
    }

    /// <summary>
    /// Get the next available default model for text/function-calling.
    /// </summary>
    public string GetNextDefaultModel()
    {
        if (_defaultModels.Length == 0)
            return _defaultModels[0]; // Fallback to first if somehow empty

        var totalModels = _defaultModels.Length;

        // Try each model once via round-robin
        for (var attempt = 0; attempt < totalModels; attempt++)
        {
            var index = Interlocked.Increment(ref _defaultModelIndex) % totalModels;
            if (index < 0) index += totalModels; // Handle negative modulo edge case

            var model = _defaultModels[index];
            var state = _modelStates.GetOrAdd(model, _ => new ModelState());

            if (state.IsAvailable)
            {
                _logger.LogDebug("Using default model: {Model} (slot {Index})", model, index);
                return model;
            }

            // Check if cooldown expired
            if (state.CooldownUntil.HasValue && DateTime.UtcNow >= state.CooldownUntil.Value)
            {
                state.Reset();
                _logger.LogInformation("Model {Model} cooldown expired, re-enabling", model);
                return model;
            }
        }

        // All models cooling down — return first available after cooldown, or first model
        _logger.LogWarning("All {Count} default models are exhausted or cooling down", totalModels);
        return _defaultModels[0];
    }

    /// <summary>
    /// Get the next available vision model for image analysis.
    /// </summary>
    public string GetNextVisionModel()
    {
        if (_visionModels.Length == 0)
            return _visionModels[0]; // Fallback

        var totalModels = _visionModels.Length;

        for (var attempt = 0; attempt < totalModels; attempt++)
        {
            var index = Interlocked.Increment(ref _visionModelIndex) % totalModels;
            if (index < 0) index += totalModels;

            var model = _visionModels[index];
            var state = _modelStates.GetOrAdd(model, _ => new ModelState());

            if (state.IsAvailable)
            {
                _logger.LogDebug("Using vision model: {Model} (slot {Index})", model, index);
                return model;
            }

            if (state.CooldownUntil.HasValue && DateTime.UtcNow >= state.CooldownUntil.Value)
            {
                state.Reset();
                _logger.LogInformation("Model {Model} cooldown expired, re-enabling", model);
                return model;
            }
        }

        _logger.LogWarning("All {Count} vision models are exhausted or cooling down", totalModels);
        return _visionModels[0];
    }

    /// <summary>
    /// Mark a model as exhausted and enter cooldown.
    /// </summary>
    public void MarkModelExhausted(string model, TimeSpan cooldownDuration)
    {
        var state = _modelStates.GetOrAdd(model, _ => new ModelState());
        state.IsAvailable = false;
        state.CooldownUntil = DateTime.UtcNow.Add(cooldownDuration);
        state.FailureCount++;

        _logger.LogWarning(
            "Model {Model} marked as exhausted. Cooldown until {CooldownUntil}. Failures: {Failures}",
            model, state.CooldownUntil, state.FailureCount);
    }

    /// <summary>
    /// Mark a model as successful (reset failure counter).
    /// </summary>
    public void MarkModelSuccess(string model)
    {
        var state = _modelStates.GetOrAdd(model, _ => new ModelState());
        state.SuccessCount++;
    }

    /// <summary>
    /// Get status of all models.
    /// </summary>
    public IReadOnlyList<ModelStatus> GetModelStatuses()
    {
        return _defaultModels.Concat(_visionModels).Distinct().Select(model =>
        {
            var state = _modelStates.GetOrAdd(model, _ => new ModelState());
            var isAvailable = state.IsAvailable ||
                              (state.CooldownUntil.HasValue && DateTime.UtcNow >= state.CooldownUntil.Value);

            return new ModelStatus
            {
                Model = model,
                IsAvailable = isAvailable,
                CooldownUntil = state.CooldownUntil,
                SuccessCount = state.SuccessCount,
                FailureCount = state.FailureCount
            };
        }).ToList();
    }

    private sealed class ModelState
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

/// <summary>
/// Status of a single Groq model.
/// </summary>
public sealed class ModelStatus
{
    public required string Model { get; init; }
    public bool IsAvailable { get; init; }
    public DateTime? CooldownUntil { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
}
