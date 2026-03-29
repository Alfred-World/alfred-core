using Alfred.Core.Application.Attachments.Dtos;
using Alfred.Core.Application.Files;
using Alfred.Core.Domain.Abstractions.Services;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Attachments;

public sealed class AttachmentService : IAttachmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly IStorageService _storageService;
    private readonly IStorageSettings _settings;

    public AttachmentService(
        IUnitOfWork unitOfWork,
        IFileService fileService,
        IStorageService storageService,
        IStorageSettings settings)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _storageService = storageService;
        _settings = settings;
    }

    public async Task<AttachmentDto> UploadAttachmentAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        CreateAttachmentDto dto,
        CancellationToken cancellationToken = default)
    {
        // Determine folder from target type + purpose
        var folder = BuildFolder(dto.TargetType, dto.Purpose);

        // Upload to R2 via the existing file service (validates size/type, generates key)
        var uploadResult = await _fileService.UploadFileProxyAsync(
            fileStream, fileName, contentType, fileSize, folder, cancellationToken);

        // Create the DB record
        var entity = Attachment.Create(
            dto.TargetId,
            dto.TargetType,
            uploadResult.ObjectKey,
            fileName,
            contentType,
            fileSize,
            dto.Purpose);

        await _unitOfWork.Attachments.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate a signed download URL for the response
        var downloadUrl = await GenerateSignedUrl(entity.ObjectKey, cancellationToken);

        return ToDto(entity, downloadUrl);
    }

    public async Task<List<AttachmentDto>> GetAttachmentsByTargetAsync(
        Guid targetId,
        string targetType,
        CancellationToken cancellationToken = default)
    {
        var attachments = await _unitOfWork.Attachments.GetByTargetAsync(targetId, targetType, cancellationToken);

        if (attachments.Count == 0)
        {
            return [];
        }

        // Batch all signed URL requests in parallel to avoid N+1 HTTP calls to storage
        var signedUrlTasks = attachments
            .Select(a => GenerateSignedUrl(a.ObjectKey, cancellationToken))
            .ToList();

        var signedUrls = await Task.WhenAll(signedUrlTasks);

        return attachments
            .Zip(signedUrls, (attachment, url) => ToDto(attachment, url))
            .ToList();
    }

    public async Task DeleteAttachmentAsync(AttachmentId attachmentId, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Attachments.GetByIdAsync(attachmentId, cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Attachment with ID {attachmentId} not found.");
        }

        // Delete from R2
        await _storageService.DeleteObjectAsync(entity.ObjectKey, cancellationToken);

        // Delete DB record
        _unitOfWork.Attachments.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private async Task<string> GenerateSignedUrl(string objectKey, CancellationToken cancellationToken)
    {
        return await _storageService.GeneratePresignedDownloadUrlAsync(
            objectKey, _settings.DownloadUrlExpirationMinutes, cancellationToken);
    }

    private static AttachmentDto ToDto(Attachment entity, string downloadUrl)
    {
        return new AttachmentDto(
            entity.Id,
            entity.TargetId,
            entity.TargetType,
            entity.FileName,
            entity.ContentType,
            entity.FileSize,
            entity.Purpose,
            downloadUrl,
            entity.CreatedAt);
    }

    private static string BuildFolder(string targetType, string purpose)
    {
        var type = targetType.ToLowerInvariant();
        var purposeFolder = purpose.ToLowerInvariant() switch
        {
            "primaryimage" => "images",
            _ => "attachments"
        };

        return $"{type}s/{purposeFolder}";
    }
}
