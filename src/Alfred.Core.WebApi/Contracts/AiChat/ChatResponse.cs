namespace Alfred.Core.WebApi.Contracts.AiChat;

/// <summary>
/// Chat response returned to the frontend.
/// </summary>
public sealed record ChatResponse
{
    /// <summary>
    /// Whether the AI processing succeeded.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Response type: "text" for a plain reply, "action" for function execution results.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// AI's text reply or summary of actions executed.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Detailed results of executed functions (when Type is "action").
    /// Frontend can render these as Action Cards for user review.
    /// </summary>
    public List<ActionResultEntry>? Actions { get; init; }

    /// <summary>
    /// Error detail when IsSuccess is false.
    /// </summary>
    public string? Error { get; init; }
}

/// <summary>
/// Result of a single AI-dispatched function execution.
/// Frontend can render this as an Action Card (preview before saving).
/// </summary>
public sealed record ActionResultEntry
{
    public required string FunctionName { get; init; }
    public required bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public object? Data { get; init; }
    public string? Error { get; init; }
}
