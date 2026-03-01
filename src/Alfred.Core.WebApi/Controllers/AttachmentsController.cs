using Alfred.Core.Application.Attachments;
using Alfred.Core.Application.Attachments.Dtos;
using Alfred.Core.WebApi.Contracts.Common;

using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

/// <summary>
/// Polymorphic attachment API — upload, list, and delete files for any entity.
/// All download URLs returned are time-limited presigned URLs (never raw R2 paths).
/// </summary>
[Route("api/v{version:apiVersion}/attachments")]
public sealed class AttachmentsController : BaseApiController
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>
    /// Upload a file and attach it to a target entity.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(52_428_800)] // 50 MB
    [ProducesResponseType(typeof(ApiResponse<AttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] Guid targetId,
        [FromForm] string targetType,
        [FromForm] string? purpose,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequestResponse("No file provided.", "NO_FILE");

        try
        {
            await using var stream = file.OpenReadStream();

            var dto = new CreateAttachmentDto(
                targetId,
                targetType,
                purpose ?? "Attachment");

            var result = await _attachmentService.UploadAttachmentAsync(
                stream, file.FileName, file.ContentType, file.Length, dto, cancellationToken);

            return OkResponse(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequestResponse(ex.Message, "INVALID_FILE");
        }
    }

    /// <summary>
    /// List all attachments for a target entity. URLs are presigned (time-limited).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AttachmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTarget(
        [FromQuery] Guid targetId,
        [FromQuery] string targetType,
        CancellationToken cancellationToken)
    {
        var result = await _attachmentService.GetAttachmentsByTargetAsync(
            targetId, targetType, cancellationToken);

        return OkResponse(result);
    }

    /// <summary>
    /// Delete an attachment (removes DB record and R2 object).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _attachmentService.DeleteAttachmentAsync(id, cancellationToken);
            return OkResponse("Attachment deleted successfully");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundResponse(ex.Message);
        }
    }
}
