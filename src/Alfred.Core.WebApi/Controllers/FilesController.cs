using Alfred.Core.Application.Files;
using Alfred.Core.Application.Files.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Contracts.Files;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

/// <summary>
/// Handles file upload operations via Cloudflare R2 presigned URLs.
/// Files are uploaded directly from client to R2 — the server only generates signed URLs.
/// </summary>
[Route("api/v{version:apiVersion}/files")]
[Authorize]
public sealed class FilesController : BaseApiController
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Generate a presigned URL for direct file upload to Cloudflare R2.
    /// Flow: FE calls this → gets presigned URL → FE PUTs file directly to R2.
    /// </summary>
    [HttpPost("upload-url")]
    [RequirePermission(PermissionCodes.File.Create)]
    [ProducesResponseType(typeof(ApiResponse<UploadUrlResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateUploadUrl(
        [FromBody] GenerateUploadUrlRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fileService.GenerateUploadUrlAsync(request.ToDto(), cancellationToken);
            return OkResponse(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestResponse(ex.Message, "STORAGE_QUOTA_EXCEEDED");
        }
        catch (ArgumentException ex)
        {
            return BadRequestResponse(ex.Message, "INVALID_FILE");
        }
    }

    /// <summary>
    /// Generate a presigned URL for downloading/viewing a file from R2.
    /// </summary>
    [HttpPost("download-url")]
    [RequirePermission(PermissionCodes.File.Read)]
    [ProducesResponseType(typeof(ApiResponse<DownloadUrlResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateDownloadUrl(
        [FromBody] GenerateDownloadUrlRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fileService.GenerateDownloadUrlAsync(request.ToDto(), cancellationToken);
            return OkResponse(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequestResponse(ex.Message, "INVALID_REQUEST");
        }
    }

    /// <summary>
    /// Delete a file from R2 storage.
    /// </summary>
    [HttpDelete]
    [RequirePermission(PermissionCodes.File.Delete)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteFile(
        [FromBody] DeleteFileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _fileService.DeleteFileAsync(request.ToDto(), cancellationToken);
            return OkResponse("File deleted successfully");
        }
        catch (ArgumentException ex)
        {
            return BadRequestResponse(ex.Message, "INVALID_REQUEST");
        }
    }

    /// <summary>
    /// Upload a file to R2 via server-side proxy (avoids browser CORS on direct R2 uploads).
    /// Accepts multipart/form-data with a 'file' field and optional 'folder' field.
    /// </summary>
    [HttpPost("upload")]
    [RequirePermission(PermissionCodes.File.Create)]
    [RequestSizeLimit(52_428_800)] // 50 MB
    [ProducesResponseType(typeof(ApiResponse<FileUploadResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadFile(
        IFormFile file,
        [FromForm] string? folder,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequestResponse("No file provided.", "NO_FILE");
        }

        try
        {
            await using var stream = file.OpenReadStream();

            var result = await _fileService.UploadFileProxyAsync(
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                folder,
                cancellationToken);

            return OkResponse(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestResponse(ex.Message, "STORAGE_QUOTA_EXCEEDED");
        }
        catch (ArgumentException ex)
        {
            return BadRequestResponse(ex.Message, "INVALID_FILE");
        }
    }
}
