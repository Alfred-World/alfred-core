using Alfred.Core.Application.Files;
using Alfred.Core.Application.Files.Dtos;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Contracts.Files;

using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

/// <summary>
/// Handles file upload operations via Cloudflare R2 presigned URLs.
/// Files are uploaded directly from client to R2 — the server only generates signed URLs.
/// </summary>
[Route("api/v{version:apiVersion}/files")]
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
        catch (ArgumentException ex)
        {
            return BadRequestResponse(ex.Message, "INVALID_FILE");
        }
    }

    /// <summary>
    /// Generate a presigned URL for downloading/viewing a file from R2.
    /// </summary>
    [HttpPost("download-url")]
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
}
