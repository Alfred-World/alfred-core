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

    public FileService(IStorageService storageService, IStorageSettings settings)
    {
        _storageService = storageService;
        _settings = settings;
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

        // Generate a unique object key with folder structure
        var objectKey = GenerateObjectKey(dto.FileName, dto.Folder);
        var expirationMinutes = _settings.UploadUrlExpirationMinutes;

        var uploadUrl = await _storageService.GeneratePresignedUploadUrlAsync(
            objectKey, dto.ContentType, expirationMinutes, cancellationToken);

        return new UploadUrlResultDto(
            UploadUrl: uploadUrl,
            ObjectKey: objectKey,
            ExpiresAt: DateTime.UtcNow.AddMinutes(expirationMinutes));
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
            DownloadUrl: downloadUrl,
            ExpiresAt: DateTime.UtcNow.AddMinutes(expirationMinutes));
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
        var sanitized = System.Text.RegularExpressions.Regex.Replace(name, @"[^\w\-.]", "_");

        // Limit length
        if (sanitized.Length > 100)
        {
            sanitized = sanitized[..100];
        }

        return $"{sanitized}{ext}".ToLowerInvariant();
    }
}
