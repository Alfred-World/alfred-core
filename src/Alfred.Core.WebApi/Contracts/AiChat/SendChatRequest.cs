namespace Alfred.Core.WebApi.Contracts.AiChat;

/// <summary>
/// Chat request sent from the frontend to the AI chat endpoint.
/// Context is maintained on the frontend (in-memory) and sent with each request.
/// When the user closes the browser or refreshes, context is lost — by design.
/// </summary>
public sealed record SendChatRequest
{
    /// <summary>
    /// Recent conversation messages kept in frontend memory.
    /// Sent to provide conversational context for the AI.
    /// The frontend should trim this to the most recent N messages.
    /// </summary>
    public List<ChatMessageEntry> Context { get; init; } = [];

    /// <summary>
    /// The current user message or command.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional base64-encoded image attachment (e.g., receipt, document, report photo).
    /// </summary>
    public string? ImageBase64 { get; init; }

    /// <summary>
    /// MIME type of the image attachment. Defaults to image/jpeg.
    /// </summary>
    public string ImageMimeType { get; init; } = "image/jpeg";
}

/// <summary>
/// A single message entry in the conversation context.
/// </summary>
public sealed record ChatMessageEntry
{
    /// <summary>
    /// Message role: "user" or "assistant".
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Message text content.
    /// </summary>
    public required string Content { get; init; }
}
