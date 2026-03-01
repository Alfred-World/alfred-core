using Alfred.Core.Domain.Abstractions.Services;
using Alfred.Core.Infrastructure.Common.Options;

using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.Extensions.Logging;

namespace Alfred.Core.Infrastructure.Services;

/// <summary>
/// Cloudflare R2 implementation of IStorageService using the S3-compatible API.
/// </summary>
public sealed class R2StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly R2StorageOptions _options;
    private readonly ILogger<R2StorageService> _logger;

    public R2StorageService(
        IAmazonS3 s3Client,
        R2StorageOptions options,
        ILogger<R2StorageService> logger)
    {
        _s3Client = s3Client;
        _options = options;
        _logger = logger;
    }

    public async Task<string> GeneratePresignedUploadUrlAsync(
        string objectKey,
        string contentType,
        int expirationInMinutes = 5,
        CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(expirationInMinutes),
            ContentType = contentType
        };

        var url = await _s3Client.GetPreSignedURLAsync(request);

        _logger.LogDebug("Generated presigned upload URL for key: {ObjectKey}, expires in {Minutes} minutes",
            objectKey, expirationInMinutes);

        return url;
    }

    public async Task<string> GeneratePresignedDownloadUrlAsync(
        string objectKey,
        int expirationInMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(expirationInMinutes)
        };

        var url = await _s3Client.GetPreSignedURLAsync(request);

        _logger.LogDebug("Generated presigned download URL for key: {ObjectKey}, expires in {Minutes} minutes",
            objectKey, expirationInMinutes);

        return url;
    }

    public async Task DeleteObjectAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken);

        _logger.LogInformation("Deleted object from R2: {ObjectKey}", objectKey);
    }

    public async Task UploadFileAsync(
        Stream fileStream,
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            InputStream = fileStream,
            ContentType = contentType,
            DisablePayloadSigning = true
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        _logger.LogInformation("Uploaded file to R2: {ObjectKey}", objectKey);
    }
}
