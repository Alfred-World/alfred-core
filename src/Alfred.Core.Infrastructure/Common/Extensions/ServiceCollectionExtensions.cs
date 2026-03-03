using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Abstractions.Services;
using Alfred.Core.Infrastructure.Common.HealthChecks;
using Alfred.Core.Infrastructure.Common.Options;
using Alfred.Core.Infrastructure.Common.Seeding;
using Alfred.Core.Infrastructure.Providers.Cache;
using Alfred.Core.Infrastructure.Providers.Cache.HealthChecks;
using Alfred.Core.Infrastructure.Providers.PostgreSQL;
using Alfred.Core.Infrastructure.Providers.SqlServer.HealthChecks;
using Alfred.Core.Infrastructure.Providers.Storage.HealthChecks;
using Alfred.Core.Infrastructure.Repositories;
using Alfred.Core.Infrastructure.Services;

using Amazon.Runtime;
using Amazon.S3;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alfred.Core.Infrastructure.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IAssetLogRepository, AssetLogRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICommodityRepository, CommodityRepository>();
        services.AddScoped<IInvestmentTransactionRepository, InvestmentTransactionRepository>();
        services.AddScoped<IUnitRepository, UnitRepository>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Caching
        services.AddInMemoryCache();

        // Location Services
        services.AddHttpClient<IpApiLocationService>();
        services.AddScoped<ILocationService, IpApiLocationService>();

        // Other Services
        services.AddScoped<IAuthorizationCodeService, AuthorizationCodeService>();

        // Health Checks — registered before the orchestrator so it can enumerate them
        services.AddScoped<IHealthCheck, DatabaseHealthCheck>();
        services.AddScoped<IHealthCheck, RedisHealthCheck>();
        services.AddScoped<IHealthCheck, R2StorageHealthCheck>();

        // Orchestrators
        services.AddScoped<HealthCheckOrchestrator>();

        // Data Seeding
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        services.AddScoped(sp => new DataSeederOrchestrator(
            sp,
            sp.GetRequiredService<ILogger<DataSeederOrchestrator>>(),
            environment));

        // R2 Storage
        services.AddR2Storage();

        return services;
    }

    /// <summary>
    /// Register Cloudflare R2 storage services (S3-compatible).
    /// Reads configuration from environment variables.
    /// </summary>
    public static IServiceCollection AddR2Storage(this IServiceCollection services)
    {
        var options = new R2StorageOptions
        {
            AccountId = Environment.GetEnvironmentVariable("R2_ACCOUNT_ID") ?? "",
            AccessKeyId = Environment.GetEnvironmentVariable("R2_ACCESS_KEY_ID") ?? "",
            SecretAccessKey = Environment.GetEnvironmentVariable("R2_SECRET_ACCESS_KEY") ?? "",
            BucketName = Environment.GetEnvironmentVariable("R2_BUCKET_NAME") ?? ""
        };

        if (int.TryParse(Environment.GetEnvironmentVariable("R2_UPLOAD_URL_EXPIRATION_MINUTES"), out var uploadExp))
        {
            options.UploadUrlExpirationMinutes = uploadExp;
        }

        if (int.TryParse(Environment.GetEnvironmentVariable("R2_DOWNLOAD_URL_EXPIRATION_MINUTES"), out var downloadExp))
        {
            options.DownloadUrlExpirationMinutes = downloadExp;
        }

        if (long.TryParse(Environment.GetEnvironmentVariable("R2_MAX_FILE_SIZE_BYTES"), out var maxSize))
        {
            options.MaxFileSizeBytes = maxSize;
        }

        var allowedTypes = Environment.GetEnvironmentVariable("R2_ALLOWED_CONTENT_TYPES");
        if (!string.IsNullOrWhiteSpace(allowedTypes))
        {
            options.AllowedContentTypes = allowedTypes.Split(',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        options.CloudflareApiToken = Environment.GetEnvironmentVariable("CF_API_TOKEN") ?? string.Empty;

        if (long.TryParse(Environment.GetEnvironmentVariable("R2_STORAGE_QUOTA_BYTES"), out var quotaBytes))
        {
            options.StorageQuotaBytes = quotaBytes;
        }

        options.Validate();

        // Register options as singleton (concrete + interface)
        services.AddSingleton(options);
        services.AddSingleton<IStorageSettings>(options);

        // Register S3 client configured for Cloudflare R2
        services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = options.Endpoint,
                ForcePathStyle = true
            };
            var credentials = new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey);
            return new AmazonS3Client(credentials, config);
        });

        // Register storage service
        services.AddScoped<IStorageService, R2StorageService>();

        // Register Cloudflare Metrics service (typed HttpClient)
        services.AddHttpClient<CloudflareR2MetricsService>();
        services.AddScoped<IR2MetricsService, CloudflareR2MetricsService>();

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        // Get database provider - REQUIRED, no default
        var providerStr = Environment.GetEnvironmentVariable("DB_PROVIDER");
        if (string.IsNullOrEmpty(providerStr))
        {
            throw new InvalidOperationException(
                "DB_PROVIDER environment variable is required. Valid value: 'PostgreSQL'");
        }

        // Validate provider
        if (!Enum.TryParse<DatabaseProvider>(providerStr, true, out var provider))
        {
            throw new InvalidOperationException(
                $"Invalid DB_PROVIDER value: '{providerStr}'. Valid value: 'PostgreSQL'");
        }

        // Get database connection parameters
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "alfred_identity";
        var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres";

        // Build connection string based on provider
        string connectionString;
        switch (provider)
        {
            case DatabaseProvider.PostgreSQL:
                connectionString =
                    $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};";
                break;

            default:
                throw new InvalidOperationException($"Unsupported database provider: {provider}");
        }

        // EF Core diagnostics — read from environment (default: off for production safety)
        var enableDetailedErrors = string.Equals(
            Environment.GetEnvironmentVariable("ENABLE_DETAILED_ERRORS"), "true",
            StringComparison.OrdinalIgnoreCase);
        var enableSensitiveDataLogging = string.Equals(
            Environment.GetEnvironmentVariable("ENABLE_SENSITIVE_DATA_LOGGING"), "true",
            StringComparison.OrdinalIgnoreCase);

        // Register PostgreSQL database provider
        var pgOptions = new PostgreSqlOptions
        {
            ConnectionString = connectionString,
            EnableDetailedErrors = enableDetailedErrors,
            EnableSensitiveDataLogging = enableSensitiveDataLogging
        };
        services.AddPostgreSQL(pgOptions);


        return services;
    }
}
