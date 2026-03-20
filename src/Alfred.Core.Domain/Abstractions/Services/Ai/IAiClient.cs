namespace Alfred.Core.Domain.Abstractions.Services.Ai;

/// <summary>
/// Abstraction for an AI/LLM client that supports chat completion with optional vision and function calling.
/// </summary>
public interface IAiClient
{
    /// <summary>
    /// Send a chat completion request to the AI provider.
    /// </summary>
    Task<AiChatResponse> ChatCompletionAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a single message in the AI conversation.
/// </summary>
public sealed class AiMessage
{
    public required string Role { get; init; }
    public required IReadOnlyList<AiContentPart> Content { get; init; }

    public static AiMessage System(string text) => new()
    {
        Role = "system",
        Content = [AiContentPart.Text(text)]
    };

    public static AiMessage User(string text) => new()
    {
        Role = "user",
        Content = [AiContentPart.Text(text)]
    };

    public static AiMessage UserWithImage(string text, string base64Image, string mimeType = "image/jpeg") => new()
    {
        Role = "user",
        Content = [AiContentPart.Text(text), AiContentPart.Image(base64Image, mimeType)]
    };

    public static AiMessage Assistant(string text) => new()
    {
        Role = "assistant",
        Content = [AiContentPart.Text(text)]
    };
}

/// <summary>
/// A content part within a message — either text or an image.
/// </summary>
public sealed class AiContentPart
{
    public required string Type { get; init; }
    public string? TextValue { get; init; }
    public AiImageUrl? ImageUrl { get; init; }

    public static AiContentPart Text(string text) => new() { Type = "text", TextValue = text };

    public static AiContentPart Image(string base64Data, string mimeType = "image/jpeg") => new()
    {
        Type = "image_url",
        ImageUrl = new AiImageUrl { Url = $"data:{mimeType};base64,{base64Data}" }
    };
}

public sealed class AiImageUrl
{
    public required string Url { get; init; }
}

/// <summary>
/// Chat completion request payload.
/// </summary>
public sealed class AiChatRequest
{
    public required string Model { get; init; }
    public required IReadOnlyList<AiMessage> Messages { get; init; }
    public IReadOnlyList<AiFunctionDefinition>? Tools { get; init; }
    public string? ToolChoice { get; init; }
    public float Temperature { get; init; } = 0.1f;
    public int MaxTokens { get; init; } = 4096;
}

/// <summary>
/// Defines a function the AI can call.
/// </summary>
public sealed class AiFunctionDefinition
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required object Parameters { get; init; }
}

/// <summary>
/// Chat completion response from the AI provider.
/// </summary>
public sealed class AiChatResponse
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public string? TextContent { get; init; }
    public IReadOnlyList<AiToolCall>? ToolCalls { get; init; }

    public bool HasToolCalls => ToolCalls is { Count: > 0 };
}

/// <summary>
/// A tool/function call output from the AI.
/// </summary>
public sealed class AiToolCall
{
    public required string Id { get; init; }
    public required string FunctionName { get; init; }
    public required string ArgumentsJson { get; init; }
}
