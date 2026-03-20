using Alfred.Core.Application.AiFunctions;
using Alfred.Core.Domain.Abstractions.Services.Ai;
using Alfred.Core.WebApi.Contracts.AiChat;
using Alfred.Core.WebApi.Contracts.Common;

using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

/// <summary>
/// AI chat endpoint — the frontend sends a message (+ optional image + conversation context)
/// and receives either a text reply or a list of executed function results.
/// </summary>
[Route("api/v{version:apiVersion}/ai/chat")]
public sealed class AiChatController : BaseApiController
{
    private readonly IAiFunctionCallService _aiFunctionCallService;

    public AiChatController(IAiFunctionCallService aiFunctionCallService)
    {
        _aiFunctionCallService = aiFunctionCallService;
    }

    /// <summary>
    /// Send a chat message to the AI assistant.
    /// The frontend manages conversation context in-memory and sends it along with each request.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ChatResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequestResponse("Message is required.");
        }

        // Convert frontend context to domain AiMessage list
        var context = request.Context.Count > 0
            ? request.Context.ConvertAll(MapToAiMessage)
            : null;

        AiFunctionCallResult result;

        if (!string.IsNullOrWhiteSpace(request.ImageBase64))
        {
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(request.ImageBase64);
            }
            catch (FormatException)
            {
                return BadRequestResponse("Invalid Base64 image data.");
            }

            result = await _aiFunctionCallService.ProcessCommandWithImageAsync(
                request.Message,
                imageBytes,
                request.ImageMimeType,
                context,
                cancellationToken);
        }
        else
        {
            result = await _aiFunctionCallService.ProcessCommandAsync(
                request.Message,
                context,
                cancellationToken);
        }

        var response = MapToResponse(result);
        return OkResponse(response);
    }

    private static AiMessage MapToAiMessage(ChatMessageEntry entry)
    {
        return entry.Role.ToLowerInvariant() switch
        {
            "assistant" => AiMessage.Assistant(entry.Content),
            _ => AiMessage.User(entry.Content)
        };
    }

    private static ChatResponse MapToResponse(AiFunctionCallResult result)
    {
        if (!result.IsSuccess)
        {
            return new ChatResponse
            {
                IsSuccess = false,
                Type = "text",
                Error = result.Error
            };
        }

        var hasActions = result.ExecutedFunctions is { Count: > 0 };

        return new ChatResponse
        {
            IsSuccess = true,
            Type = hasActions ? "action" : "text",
            Message = result.Message,
            Actions = hasActions
                ? result.ExecutedFunctions!.Select(f => new ActionResultEntry
                {
                    FunctionName = f.FunctionName,
                    IsSuccess = f.IsSuccess,
                    Message = f.Message,
                    Data = f.Data,
                    Error = f.Error
                }).ToList()
                : null
        };
    }
}
