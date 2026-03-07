using System.Diagnostics;
using System.Net;

using Alfred.Core.Infrastructure.Common.HealthChecks;
using Alfred.Core.Infrastructure.Common.Options;

using Amazon.S3;
using Amazon.S3.Model;

namespace Alfred.Core.Infrastructure.Providers.Storage.HealthChecks;

/// <summary>
/// Health check for Cloudflare R2 (S3-compatible) storage.
/// Issues a HeadBucket request to validate credentials and bucket accessibility
/// using the already-configured IAmazonS3 singleton.
/// </summary>
public sealed class R2StorageHealthCheck : IHealthCheck
{
    private readonly IAmazonS3 _s3Client;
    private readonly R2StorageOptions _options;

    public string ServiceName => "R2 Storage";

    public R2StorageHealthCheck(IAmazonS3 s3Client, R2StorageOptions options)
    {
        _s3Client = s3Client;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // HeadBucket is the lightest call — no data transferred, just verifies
            // that credentials are valid and the bucket is accessible
            var request = new GetBucketLocationRequest
            {
                BucketName = _options.BucketName
            };

            await _s3Client.GetBucketLocationAsync(request, cancellationToken);

            stopwatch.Stop();
            return HealthCheckResult.Healthy(
                ServiceName,
                $"R2 bucket '{_options.BucketName}' is accessible at {_options.Endpoint}",
                stopwatch.Elapsed);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            stopwatch.Stop();
            return HealthCheckResult.Unhealthy(
                ServiceName,
                $"R2 access denied for bucket '{_options.BucketName}' — check R2_ACCESS_KEY_ID and R2_SECRET_ACCESS_KEY",
                stopwatch.Elapsed);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            stopwatch.Stop();
            return HealthCheckResult.Unhealthy(
                ServiceName,
                $"R2 bucket '{_options.BucketName}' does not exist — check R2_BUCKET_NAME and R2_ACCOUNT_ID",
                stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return HealthCheckResult.Unhealthy(
                ServiceName,
                $"R2 storage health check failed: {ex.Message}",
                stopwatch.Elapsed);
        }
    }
}
