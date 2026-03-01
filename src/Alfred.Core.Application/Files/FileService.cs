using System.Text.RegularExpressions;

using Alfred.Core.Application.Files.Dtos;
using Alfred.Core.Domain.Abstractions.Services;

namespace Alfred.Core.Application.Files;

/// <summary>
/// Application service that handles file upload orchestration:
/// validates requests, generates unique keys, and delegates to IStorageService.
/// </summary>
public sealed class FileService : IFileService
{
    private readonly IStorageService _storageService;
    private readonly IStorageSettings _settings;
    private readonly IR2MetricsService _metricsService;

    public FileService(
        IStorageService storageService,
        IStorageSettings settings,
        IR2MetricsService metricsService)
    {
        _storageService = storageService;
        _settings = settings;
        _metricsService = metricsService;
    }

    public async Task<UploadUrlResultDto> GenerateUploadUrlAsync(
        GenerateUploadUrlDto dto,
        CancellationToken cancellationToken = default)
    {
        // Validate file size
        if (dto.FileSize > _settings.MaxFileSizeBytes)
        {
            throw new ArgumentException(
                $"File size {dto.FileSize} bytes exceeds maximum allowed size of {_settings.MaxFileSizeBytes} bytes.");
        }

        // Validate content type if restrictions are configured
        if (_settings.AllowedContentTypes.Length > 0 &&
            !_settings.AllowedContentTypes.Contains(dto.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Content type '{dto.ContentType}' is not allowed. Allowed types: {string.Join(", ", _settings.AllowedContentTypes)}");
        }

        // Check Cloudflare R2 storage quota (estimatedUsageBytes via GraphQL API)
        await CheckStorageQuotaAsync(dto.FileSize, cancellationToken);

        // Generate a unique object key with folder structure
        var objectKey = GenerateObjectKey(dto.FileName, dto.Folder);
        var expirationMinutes = _settings.UploadUrlExpirationMinutes;

        var uploadUrl = await _storageService.GeneratePresignedUploadUrlAsync(
            objectKey, dto.ContentType, expirationMinutes, cancellationToken);

        return new UploadUrlResultDto(
            uploadUrl,
            objectKey,
            DateTime.UtcNow.AddMinutes(expirationMinutes));
    }

    public async Task<DownloadUrlResultDto> GenerateDownloadUrlAsync(
        GenerateDownloadUrlDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ObjectKey))
        {
            throw new ArgumentException("Object key is required.");
        }

        var expirationMinutes = _settings.DownloadUrlExpirationMinutes;

        var downloadUrl = await _storageService.GeneratePresignedDownloadUrlAsync(
            dto.ObjectKey, expirationMinutes, cancellationToken);

        return new DownloadUrlResultDto(
            downloadUrl,
            DateTime.UtcNow.AddMinutes(expirationMinutes));
    }

    public async Task DeleteFileAsync(
        DeleteFileDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ObjectKey))
        {
            throw new ArgumentException("Object key is required.");
        }

        await _storageService.DeleteObjectAsync(dto.ObjectKey, cancellationToken);
    }

    public async Task<FileUploadResultDto> UploadFileProxyAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        string? folder,
        CancellationToken cancellationToken = default)
    {
        if (fileSize > _settings.MaxFileSizeBytes)
        {
            throw new ArgumentException(
                $"File size {fileSize} bytes exceeds maximum allowed size of {_settings.MaxFileSizeBytes} bytes.");
        }

        if (_settings.AllowedContentTypes.Length > 0 &&
            !_settings.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Content type '{contentType}' is not allowed.");
        }

        // Check Cloudflare R2 storage quota (estimatedUsageBytes via GraphQL API)
        await CheckStorageQuotaAsync(fileSize, cancellationToken);

        var objectKey = GenerateObjectKey(fileName, folder);

        await _storageService.UploadFileAsync(fileStream, objectKey, contentType, cancellationToken);

        return new FileUploadResultDto(objectKey, fileName);
    }

    /// <summary>
    /// Generate a unique object key: {folder}/{yyyy}/{MM}/{guid}_{sanitized-filename}
    /// </summary>
    private static string GenerateObjectKey(string fileName, string? folder)
    {
        var sanitizedName = SanitizeFileName(fileName);
        var uniqueId = Guid.NewGuid().ToString("N")[..12];
        var now = DateTime.UtcNow;

        var prefix = string.IsNullOrWhiteSpace(folder)
            ? $"uploads/{now:yyyy}/{now:MM}"
            : $"{folder.Trim('/')}/{now:yyyy}/{now:MM}";

        return $"{prefix}/{uniqueId}_{sanitizedName}";
    }

    /// <summary>
    /// Remove dangerous characters from file name, keeping extension.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);

        // Replace non-alphanumeric (except - _ .) with underscore
        var sanitized = Regex.Replace(name, @"[^\w\-.]", "_");

        // Limit length
        if (sanitized.Length > 100)
        {
            sanitized = sanitized[..100];
        }

        return $"{sanitized}{ext}".ToLowerInvariant();
    }

    /// <summary>
    /// Queries Cloudflare R2 Metrics API for estimated bucket usage and throws
    /// if adding <paramref name="incomingBytes"/> would exceed the configured quota.
    /// If the API token is not set or the call fails, the check is silently skipped.
    /// </summary>
    private async Task CheckStorageQuotaAsync(long incomingBytes, CancellationToken cancellationToken)
    {
        if (_settings.StorageQuotaBytes <= 0)
        {
            return;
        }

        var estimatedUsed = await _metricsService.GetEstimatedUsageBytesAsync(cancellationToken);

        if (estimatedUsed is null)
        {
            // API token not configured or call failed — skip enforcement
            return;
        }

        if (estimatedUsed.Value + incomingBytes > _settings.StorageQuotaBytes)
        {
            var usedGb = estimatedUsed.Value / 1_073_741_824.0;
            var quotaGb = _settings.StorageQuotaBytes / 1_073_741_824.0;

            throw new InvalidOperationException(
                $"Storage quota exceeded: estimated {usedGb:F2} GB used of {quotaGb:F2} GB quota. " +
                $"(Note: Cloudflare R2 metrics may be delayed by a few minutes.)");
        }
    }
}
