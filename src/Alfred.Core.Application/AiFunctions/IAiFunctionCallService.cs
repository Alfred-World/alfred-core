using Alfred.Core.Domain.Abstractions.Services.Ai;

namespace Alfred.Core.Application.AiFunctions;

/// <summary>
/// Application-layer service that orchestrates the AI function calling pipeline:
/// 1. Accepts user input (text + optional image + conversation context from FE)
/// 2. Sends to AI with registered function definitions
/// 3. Dispatches AI tool_calls to the correct function
/// 4. Returns results
/// </summary>
public interface IAiFunctionCallService
{
    /// <summary>
    /// Process a text-only command through the AI pipeline.
    /// </summary>
    /// <param name="userMessage">Current user message.</param>
    /// <param name="context">Recent conversation history from frontend memory (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<AiFunctionCallResult> ProcessCommandAsync(
        string userMessage,
        IReadOnlyList<AiMessage>? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a command with an image through the AI pipeline.
    /// The AI uses vision to read the image and function calling to execute actions.
    /// </summary>
    /// <param name="userMessage">Current user message.</param>
    /// <param name="imageData">Raw image bytes.</param>
    /// <param name="mimeType">Image MIME type.</param>
    /// <param name="context">Recent conversation history from frontend memory (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<AiFunctionCallResult> ProcessCommandWithImageAsync(
        string userMessage,
        byte[] imageData,
        string mimeType = "image/jpeg",
        IReadOnlyList<AiMessage>? context = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of the full AI function calling pipeline.
/// </summary>
public sealed class AiFunctionCallResult
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<FunctionExecutionResult>? ExecutedFunctions { get; init; }
    public string? Error { get; init; }

    public static AiFunctionCallResult Success(string message, IReadOnlyList<FunctionExecutionResult>? functions = null)
    {
        return new AiFunctionCallResult { IsSuccess = true, Message = message, ExecutedFunctions = functions };
    }

    public static AiFunctionCallResult Failure(string error)
    {
        return new AiFunctionCallResult { IsSuccess = false, Error = error };
    }
}

/// <summary>
/// Result of a single function execution within the pipeline.
/// </summary>
public sealed class FunctionExecutionResult
{
    public required string FunctionName { get; init; }
    public required bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public object? Data { get; init; }
    public string? Error { get; init; }
}
