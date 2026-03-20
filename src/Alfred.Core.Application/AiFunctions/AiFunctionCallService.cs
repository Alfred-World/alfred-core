using System.Reflection;

using Alfred.Core.Domain.Abstractions.Services.Ai;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alfred.Core.Application.AiFunctions;

/// <summary>
/// Orchestrates the AI function calling pipeline:
/// Text/Image → Groq AI (Vision + Tool Calling) → Function dispatch → Result
/// </summary>
public sealed class AiFunctionCallService : IAiFunctionCallService
{
    private readonly IAiClient _aiClient;
    private readonly IAiFunctionRegistry _functionRegistry;
    private readonly IAiProviderSettings _aiSettings;
    private readonly ILogger<AiFunctionCallService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _systemPrompt;

    /// <summary>
    /// Load system prompt from markdown file embedded in the assembly.
    /// Falls back to a minimal prompt if file cannot be loaded.
    /// </summary>
    private static string LoadSystemPrompt()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Alfred.Core.Application.AiFunctions.Prompts.system-prompt.md";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                // Fallback: try to load from relative file path
                var assemblyPath = Path.GetDirectoryName(assembly.Location);
                if (assemblyPath != null)
                {
                    var filePath = Path.Combine(assemblyPath, "..", "..", "..", "src", "Alfred.Core.Application",
                        "AiFunctions", "Prompts", "system-prompt.md");
                    if (File.Exists(filePath))
                    {
                        return File.ReadAllText(filePath);
                    }
                }

                return GetFallbackPrompt();
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            // If file loading fails, use fallback
            Console.Error.WriteLine($"Failed to load system prompt: {ex.Message}");
            return GetFallbackPrompt();
        }
    }

    private static string GetFallbackPrompt() => """
        You are Alfred AI — an intelligent assistant for the Alfred asset management and health tracking system.

        ## General rules
        - Always use function calls when possible; never return raw JSON to the user.
        - Respond in the same language the user uses.
        - Be concise but informative in your text responses.

        ## CRITICAL — strict function–entity mapping
        Every registered function is ONLY valid for one specific entity type.
        You MUST match the user's target entity exactly before calling any function.
        - CreateBrands → ONLY for "brand" / "thương hiệu" / "nhãn hàng". NEVER for category, unit, commodity, or any other entity.
        - CreateCategories → ONLY for "category" / "danh mục" / "phân loại". NEVER for brand, unit, commodity, or any other entity.
        - If the user asks to create/update/delete an entity and NO function exists for that exact entity type,
          you MUST respond with a plain text message such as:
          "Tính năng tạo [entity] qua AI chưa được hỗ trợ." — and call NO function at all.
        - Even if two entities sound similar or the user's phrasing is ambiguous, always ask for clarification
          rather than guessing and calling the wrong function.
        Violating this rule by calling a function for the wrong entity is a critical error.

        ## Brand creation — auto-enrichment
        When the user asks to create a brand and does NOT explicitly provide website, logo, or description:
        - website: use your training knowledge to infer the official website (e.g. https://www.google.com)
        - logo_url: extract the domain from the website URL and construct:
          https://www.google.com/s2/favicons?domain={domain}&sz=128
          Examples:
          - If website is "https://www.google.com" → domain is "google.com" → logo is https://www.google.com/s2/favicons?domain=google.com&sz=128
          - If website is "https://apple.com" → domain is "apple.com" → logo is https://www.google.com/s2/favicons?domain=apple.com&sz=128
        - description: a short 1–2 sentence factual description of the brand
        Only leave a field empty if the brand is very obscure or unknown to you.

        ## Category creation — hierarchical support
        When the user asks to create categories:
        - Support single-level categories: "Create category Electronics"
        - Support hierarchical paths: "Create Electronics/Phones, Electronics/Tablets" (creates parent "Electronics" if needed)
        - If parent category doesn't exist, CreateCategories will auto-create it before creating children
        - You may provide descriptions for categories if mentioned; otherwise leave description empty
        Examples:
        - User: "Create categories A and B" → categories: [{"path": "A"}, {"path": "B"}]
        - User: "Create A/1, A/2, B/1, B/2" → categories: [{"path": "A/1"}, {"path": "A/2"}, {"path": "B/1"}, {"path": "B/2"}]
        - Always use "/" as the path separator for hierarchical categories

        ## Image processing
        When the user sends an image (receipt, document, report, etc.):
        1. Read and extract all available data from the image.
        2. Determine the user's intent (insert, update, query, etc.).
        3. Call the appropriate function with the extracted data.
        If you cannot read data from an image, explain clearly instead of guessing.
        """;

    public AiFunctionCallService(
        IAiClient aiClient,
        IAiFunctionRegistry functionRegistry,
        IAiProviderSettings aiSettings,
        IServiceProvider serviceProvider,
        ILogger<AiFunctionCallService> logger)
    {
        _aiClient = aiClient;
        _functionRegistry = functionRegistry;
        _aiSettings = aiSettings;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _systemPrompt = LoadSystemPrompt();
    }

    public async Task<AiFunctionCallResult> ProcessCommandAsync(
        string userMessage,
        IReadOnlyList<AiMessage>? context = null,
        CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(context, AiMessage.User(userMessage));

        var request = new AiChatRequest
        {
            Model = _aiSettings.DefaultModel,
            Messages = messages,
            Tools = _functionRegistry.GetAllDefinitions(),
            ToolChoice = "auto"
        };

        return await ExecutePipelineAsync(request, cancellationToken);
    }

    public async Task<AiFunctionCallResult> ProcessCommandWithImageAsync(
        string userMessage,
        byte[] imageData,
        string mimeType = "image/jpeg",
        IReadOnlyList<AiMessage>? context = null,
        CancellationToken cancellationToken = default)
    {
        var base64Image = Convert.ToBase64String(imageData);

        // Step 1: Use vision model to extract data from image
        var visionMessages = BuildMessages(context,
            AiMessage.UserWithImage(userMessage, base64Image, mimeType));

        var visionRequest = new AiChatRequest
        {
            Model = _aiSettings.VisionModel,
            Messages = visionMessages,
            Tools = _functionRegistry.GetAllDefinitions(),
            ToolChoice = "auto"
        };

        var visionResponse = await _aiClient.ChatCompletionAsync(visionRequest, cancellationToken);

        if (!visionResponse.IsSuccess)
        {
            return AiFunctionCallResult.Failure($"Vision AI failed: {visionResponse.Error}");
        }

        // If vision model returned tool calls directly, dispatch them
        if (visionResponse.HasToolCalls)
        {
            return await DispatchToolCallsAsync(visionResponse.ToolCalls!, cancellationToken);
        }

        // If vision model returned text, pass it to the function-calling model
        if (!string.IsNullOrWhiteSpace(visionResponse.TextContent))
        {
            _logger.LogDebug("Vision model returned text. Passing to function-calling model");

            var functionMessages = BuildMessages(context,
                AiMessage.User(
                    $"User request: \"{userMessage}\"\n\n" +
                    $"Extracted data from image:\n{visionResponse.TextContent}\n\n" +
                    "Please call the appropriate function with the data above."));

            var functionRequest = new AiChatRequest
            {
                Model = _aiSettings.DefaultModel,
                Messages = functionMessages,
                Tools = _functionRegistry.GetAllDefinitions(),
                ToolChoice = "auto"
            };

            return await ExecutePipelineAsync(functionRequest, cancellationToken);
        }

        return AiFunctionCallResult.Failure("AI could not extract data from the image.");
    }

    /// <summary>
    /// Build the full message list: system prompt + context history + current user message.
    /// </summary>
    private List<AiMessage> BuildMessages(IReadOnlyList<AiMessage>? context, AiMessage currentMessage)
    {
        var messages = new List<AiMessage> { AiMessage.System(_systemPrompt) };

        if (context is { Count: > 0 })
        {
            messages.AddRange(context);
        }

        messages.Add(currentMessage);
        return messages;
    }

    private async Task<AiFunctionCallResult> ExecutePipelineAsync(
        AiChatRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _aiClient.ChatCompletionAsync(request, cancellationToken);

        if (!response.IsSuccess)
        {
            return AiFunctionCallResult.Failure($"AI request failed: {response.Error}");
        }

        if (response.HasToolCalls)
        {
            return await DispatchToolCallsAsync(response.ToolCalls!, cancellationToken);
        }

        // No function calls — return the text response
        return AiFunctionCallResult.Success(
            response.TextContent ?? "AI returned no result.");
    }

    private async Task<AiFunctionCallResult> DispatchToolCallsAsync(
        IReadOnlyList<AiToolCall> toolCalls,
        CancellationToken cancellationToken)
    {
        var results = new List<FunctionExecutionResult>();

        foreach (var toolCall in toolCalls)
        {
            _logger.LogInformation("Dispatching AI function call: {FunctionName}", toolCall.FunctionName);

            var function = _functionRegistry.GetFunction(toolCall.FunctionName, _serviceProvider);
            if (function is null)
            {
                _logger.LogWarning("Unknown AI function: {FunctionName}", toolCall.FunctionName);
                results.Add(new FunctionExecutionResult
                {
                    FunctionName = toolCall.FunctionName,
                    IsSuccess = false,
                    Error = $"Function '{toolCall.FunctionName}' is not registered."
                });
                continue;
            }

            try
            {
                var result = await function.ExecuteAsync(toolCall.ArgumentsJson, cancellationToken);

                results.Add(new FunctionExecutionResult
                {
                    FunctionName = toolCall.FunctionName,
                    IsSuccess = result.IsSuccess,
                    Message = result.Message,
                    Data = result.Data,
                    Error = result.Error
                });

                _logger.LogInformation("Function {FunctionName} completed: {Success}",
                    toolCall.FunctionName, result.IsSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Function {FunctionName} threw an exception", toolCall.FunctionName);
                results.Add(new FunctionExecutionResult
                {
                    FunctionName = toolCall.FunctionName,
                    IsSuccess = false,
                    Error = $"Function execution failed: {ex.Message}"
                });
            }
        }

        var allSucceeded = results.All(r => r.IsSuccess);
        var summary = string.Join("; ",
            results.Select(r => r.IsSuccess ? r.Message : $"[FAILED] {r.FunctionName}: {r.Error}"));

        return allSucceeded
            ? AiFunctionCallResult.Success(summary, results)
            : AiFunctionCallResult.Failure(summary);
    }
}
