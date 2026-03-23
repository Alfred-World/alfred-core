using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Alfred.Core.Domain.Abstractions.Services.Ai;

using Microsoft.Extensions.Logging;

namespace Alfred.Core.Infrastructure.Providers.Ai.Groq;

/// <summary>
/// Groq API client implementing IAiClient with automatic API key and model rotation.
/// When a request fails with 429 (rate limit), quota exhaustion, or token limits,
/// the system automatically rotates to the next key and/or model.
/// </summary>
public sealed class GroqAiClient : IAiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAiKeyRotationManager _keyRotation;
    private readonly GroqModelRotationManager _modelRotation;
    private readonly GroqOptions _options;
    private readonly ILogger<GroqAiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public GroqAiClient(
        HttpClient httpClient,
        IAiKeyRotationManager keyRotation,
        GroqModelRotationManager modelRotation,
        GroqOptions options,
        ILogger<GroqAiClient> logger)
    {
        _httpClient = httpClient;
        _keyRotation = keyRotation;
        _modelRotation = modelRotation;
        _options = options;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    }

    public async Task<AiChatResponse> ChatCompletionAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var retries = 0;
        var modelAttempts = 0;

        while (retries < _options.MaxRetries)
        {
            var apiKey = _keyRotation.GetNextKey();
            if (apiKey is null)
            {
                return new AiChatResponse
                {
                    IsSuccess = false,
                    Error = "All API keys are exhausted. Please wait for cooldown or add more keys."
                };
            }

            // Reset model attempts for each key
            modelAttempts = 0;
            while (modelAttempts < _options.DefaultModels.Length)
            {
                var model = _modelRotation.GetNextDefaultModel();

                try
                {
                    // Create a new request with the selected model
                    var modelRequest = new AiChatRequest
                    {
                        Model = model,
                        Messages = request.Messages,
                        Tools = request.Tools,
                        ToolChoice = request.ToolChoice,
                        Temperature = request.Temperature,
                        MaxTokens = request.MaxTokens
                    };

                    var response = await SendRequestAsync(modelRequest, apiKey, cancellationToken);

                    if (response.IsSuccess)
                    {
                        _keyRotation.MarkKeySuccess(apiKey);
                        _modelRotation.MarkModelSuccess(model);
                        return response;
                    }

                    // Check if error is due to quota/token exhaustion on this model
                    if (IsModelExhaustionError(response.Error))
                    {
                        _modelRotation.MarkModelExhausted(model,
                            TimeSpan.FromMinutes(_options.ModelCooldownMinutes));

                        _logger.LogWarning(
                            "Model {Model} exhausted (key attempt {KeyAttempt}/{KeyMax}, model attempt {ModelAttempt}/{ModelMax}). Rotating. Error: {Error}",
                            model, retries + 1, _options.MaxRetries, modelAttempts + 1, _options.DefaultModels.Length,
                            response.Error);

                        modelAttempts++;
                        continue;
                    }

                    // Check if error is due to key exhaustion (apply key rotation)
                    if (IsKeyExhaustionError(response.Error))
                    {
                        _keyRotation.MarkKeyExhausted(apiKey,
                            TimeSpan.FromMinutes(_options.KeyCooldownMinutes));

                        _logger.LogWarning(
                            "API key exhausted (attempt {Attempt}/{Max}). Rotating to next key. Error: {Error}",
                            retries + 1, _options.MaxRetries, response.Error);

                        retries++;
                        break; // Break inner loop to try next key
                    }

                    // Non-retryable error
                    return response;
                }
                catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("Request timed out with key ...{KeySuffix} and model {Model}",
                        apiKey[^8..], model);
                    modelAttempts++;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "HTTP error with key ...{KeySuffix} and model {Model}",
                        apiKey[^8..], model);
                    modelAttempts++;
                }
            }

            // If we exhausted all models for this key, try next key
            if (modelAttempts >= _options.DefaultModels.Length)
            {
                retries++;
            }
        }

        return new AiChatResponse
        {
            IsSuccess = false,
            Error = $"All {_options.MaxRetries} retry attempts failed across keys and models. Check quotas."
        };
    }

    private async Task<AiChatResponse> SendRequestAsync(
        AiChatRequest request,
        string apiKey,
        CancellationToken cancellationToken)
    {
        var payload = BuildPayload(request);
        var jsonContent = JsonSerializer.Serialize(payload, JsonOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _logger.LogDebug("Sending Groq request to model {Model} with {MessageCount} messages",
            request.Model, request.Messages.Count);

        using var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);

        var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return new AiChatResponse
            {
                IsSuccess = false,
                Error = $"429 rate_limit: {ExtractErrorMessage(responseBody)}"
            };
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            return new AiChatResponse
            {
                IsSuccess = false,
                Error = $"{(int)httpResponse.StatusCode}: {ExtractErrorMessage(responseBody)}"
            };
        }

        return ParseResponse(responseBody);
    }

    private static object BuildPayload(AiChatRequest request)
    {
        var messages = request.Messages.Select(m => new Dictionary<string, object>
        {
            ["role"] = m.Role,
            ["content"] = m.Content.Count == 1 && m.Content[0].Type == "text"
                ? (object)m.Content[0].TextValue!
                : m.Content.Select(BuildContentPart).ToList()
        }).ToList();

        var payload = new Dictionary<string, object>
        {
            ["model"] = request.Model,
            ["messages"] = messages,
            ["temperature"] = request.Temperature,
            ["max_tokens"] = request.MaxTokens
        };

        if (request.Tools is { Count: > 0 })
        {
            payload["tools"] = request.Tools.Select(t => new Dictionary<string, object>
            {
                ["type"] = "function",
                ["function"] = new Dictionary<string, object>
                {
                    ["name"] = t.Name,
                    ["description"] = t.Description,
                    ["parameters"] = t.Parameters
                }
            }).ToList();

            if (request.ToolChoice is not null)
            {
                payload["tool_choice"] = request.ToolChoice;
            }
        }

        return payload;
    }

    private static object BuildContentPart(AiContentPart part)
    {
        if (part.Type == "image_url" && part.ImageUrl is not null)
        {
            return new Dictionary<string, object>
            {
                ["type"] = "image_url",
                ["image_url"] = new Dictionary<string, string>
                {
                    ["url"] = part.ImageUrl.Url
                }
            };
        }

        return new Dictionary<string, object>
        {
            ["type"] = "text",
            ["text"] = part.TextValue ?? ""
        };
    }

    private AiChatResponse ParseResponse(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                return new AiChatResponse
                {
                    IsSuccess = false,
                    Error = "No choices in response"
                };
            }

            var firstChoice = choices[0];
            var message = firstChoice.GetProperty("message");

            // Check for tool calls
            if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.GetArrayLength() > 0)
            {
                var calls = new List<AiToolCall>();
                foreach (var tc in toolCalls.EnumerateArray())
                {
                    var fn = tc.GetProperty("function");
                    calls.Add(new AiToolCall
                    {
                        Id = tc.GetProperty("id").GetString() ?? "",
                        FunctionName = fn.GetProperty("name").GetString() ?? "",
                        ArgumentsJson = fn.GetProperty("arguments").GetString() ?? "{}"
                    });
                }

                return new AiChatResponse
                {
                    IsSuccess = true,
                    ToolCalls = calls
                };
            }

            // Plain text response
            var content = message.TryGetProperty("content", out var contentProp)
                ? contentProp.GetString()
                : null;

            return new AiChatResponse
            {
                IsSuccess = true,
                TextContent = content
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Groq response");
            return new AiChatResponse
            {
                IsSuccess = false,
                Error = $"Failed to parse response: {ex.Message}"
            };
        }
    }

    private static string ExtractErrorMessage(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                if (error.TryGetProperty("message", out var msg))
                {
                    return msg.GetString() ?? responseBody;
                }
            }
        }
        catch
        {
            // Ignore parse errors
        }

        return responseBody;
    }

    /// <summary>
    /// Determine if an error indicates model token quota exhaustion (rotatable).
    /// </summary>
    private static bool IsModelExhaustionError(string? error)
    {
        if (string.IsNullOrEmpty(error))
        {
            return false;
        }

        var lowerError = error.ToLowerInvariant();
        return lowerError.Contains("tokens") ||
               lowerError.Contains("context_length") ||
               lowerError.Contains("length_penalty") ||
               lowerError.Contains("invalid_request_error");
    }

    /// <summary>
    /// Determine if an error indicates API key quota or rate exhaustion.
    /// </summary>
    private static bool IsKeyExhaustionError(string? error)
    {
        if (string.IsNullOrEmpty(error))
        {
            return false;
        }

        var lowerError = error.ToLowerInvariant();
        return lowerError.Contains("rate_limit") ||
               lowerError.Contains("429") ||
               lowerError.Contains("quota") ||
               lowerError.Contains("insufficient_quota");
    }
}
