using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

using Alfred.Core.Application;
using Alfred.Core.Application.AiFunctions;
using Alfred.Core.Infrastructure;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Common.HealthChecks;
using Alfred.Core.Infrastructure.Common.Seeding;
using Alfred.Core.WebApi.Configuration;
using Alfred.Core.WebApi.Extensions;
using Alfred.Core.WebApi.Middleware;

using Asp.Versioning;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Serilog;
using Serilog.Events;

// Load environment variables from .env file
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
DotEnvLoader.LoadForEnvironment(environment);

// Load and validate configuration
AppConfiguration appConfig = new();
MtlsConfiguration mtlsConfig = new();

var builder = WebApplication.CreateBuilder(args);

IList<JsonWebKey>? cachedSigningKeys = null;
var keysLastFetched = DateTime.MinValue;
var keysCacheDuration = TimeSpan.FromHours(1);
// Shared static client avoids socket exhaustion from creating new HttpClient per cache miss
var jwksHttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// Configure Kestrel to listen on the specified hostname and port from environment
builder.WebHost.ConfigureKestrel((context, options) =>
{
    if (mtlsConfig.Enabled)
    {
        // Load certificates
        var serverCert = mtlsConfig.LoadServerCertificate();
        var caCert = mtlsConfig.LoadCaCertificate();

        // HTTPS endpoint with client certificate requirement (mTLS)
        options.ListenAnyIP(mtlsConfig.HttpsPort, listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
                httpsOptions.ServerCertificate = serverCert;
                httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                httpsOptions.ClientCertificateValidation = (certificate, chain, errors) =>
                {
                    // Validate that the client certificate is signed by our CA
                    if (chain == null)
                    {
                        chain = new X509Chain();
                    }

                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                    chain.ChainPolicy.ExtraStore.Add(caCert);

                    var isValid = chain.Build(certificate);
                    if (!isValid)
                    {
                        return false;
                    }

                    // Verify the certificate chain ends with our CA
                    var chainContainsCa = chain.ChainElements
                        .Any(element => element.Certificate.Thumbprint == caCert.Thumbprint);

                    return chainContainsCa;
                };
            });
        });

        Console.WriteLine($"✅ mTLS enabled - HTTPS listening on port {mtlsConfig.HttpsPort}");

        // Optional HTTP endpoint for health checks
        if (mtlsConfig.AllowHttp)
        {
            options.ListenAnyIP(mtlsConfig.HttpPort);
            Console.WriteLine($"✅ HTTP (health checks) listening on port {mtlsConfig.HttpPort}");
        }
    }
    else
    {
        // Standard HTTP-only mode (backward compatible)
        options.ListenAnyIP(appConfig.AppPort);
        Console.WriteLine($"ℹ️ mTLS disabled - HTTP listening on port {appConfig.AppPort}");
    }
});

// Register AppConfiguration as singleton
builder.Services.AddSingleton(appConfig);
builder.Services.AddSingleton(mtlsConfig);


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new OptionalJsonConverterFactory());
    });

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
}).AddMvc().AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add FluentValidation - manual validation (no auto-validation to control error format)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    // Don't map status codes to ProblemDetails - we handle it ourselves
    options.CustomizeProblemDetails = context => { context.ProblemDetails.Extensions.Clear(); };
});

// Add Scalar API documentation
builder.Services.AddScalarConfiguration();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (appConfig.CorsAllowedOrigins.Length > 0)
        {
            policy.WithOrigins(appConfig.CorsAllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

// Add Application layer
builder.Services.AddApplication();

// Required for resolving current user claims in infrastructure services.
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("GithubApi", client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("alfred-core", "1.0"));
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
});

// Add Infrastructure layer (Database)
builder.Services.AddInfrastructure();

// Add JWT Bearer authentication (token is validated by core as defense-in-depth).
var authAuthority = Environment.GetEnvironmentVariable("AUTH_AUTHORITY") ?? "http://localhost:8100";
var authValidIssuer = Environment.GetEnvironmentVariable("AUTH_VALID_ISSUER") ?? authAuthority;
var jwksUrl = $"{authAuthority.TrimEnd('/')}/.well-known/jwks.json";

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = null;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authValidIssuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKeyResolver = (_, _, _, _) =>
            {
                if (cachedSigningKeys != null && DateTime.UtcNow - keysLastFetched < keysCacheDuration)
                {
                    return cachedSigningKeys;
                }

                try
                {
                    var response = jwksHttpClient.GetStringAsync(jwksUrl).GetAwaiter().GetResult();
                    var jwks = new JsonWebKeySet(response);
                    cachedSigningKeys = jwks.Keys.ToList();
                    keysLastFetched = DateTime.UtcNow;

                    return cachedSigningKeys;
                }
                catch
                {
                    // On JWKS fetch failure, serve cached keys if available (fail-closed otherwise)
                    return cachedSigningKeys ?? Enumerable.Empty<SecurityKey>();
                }
            }
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                if (context.Response.HasStarted)
                {
                    return;
                }

                context.HandleResponse();
                await WriteAuthErrorAsync(
                    context.Response,
                    StatusCodes.Status401Unauthorized,
                    "You are not authorized to access this resource. Please provide a valid token.",
                    "UNAUTHORIZED");
            },
            OnForbidden = async context =>
            {
                if (context.Response.HasStarted)
                {
                    return;
                }

                await WriteAuthErrorAsync(
                    context.Response,
                    StatusCodes.Status403Forbidden,
                    "You don't have permission to access this resource.",
                    "FORBIDDEN");
            }
        };
    });

builder.Services.AddAuthorization();

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Validate all services before starting the application
await ValidateServicesAsync(app.Services);

// Log application startup info
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Environment: {Environment}", appConfig.Environment);
logger.LogInformation("Listening on: http://{Hostname}:{Port}", appConfig.AppHostname, appConfig.AppPort);

// Run database migrations automatically in production
if (app.Environment.IsProduction())
{
    logger.LogInformation("Running database migrations...");
    await RunDatabaseMigrationsAsync(app.Services, logger);

    // Run data seeders (environment-aware)
    logger.LogInformation("Running data seeders...");
    await RunDataSeedersAsync(app.Services, logger);
}

// Register AI functions into the registry
app.Services.RegisterAiFunctions();

// Configure the HTTP request pipeline

// Add ForwardedHeaders middleware FIRST to properly handle X-Forwarded-* headers from YARP/Caddy
// This ensures cookies are set with the correct host (gateway.test instead of localhost)
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All,
    ForwardLimit = null // No limit on forwards
};
// Clear default known networks/proxies to trust all (for development)
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Security headers (defense-in-depth; gateway also applies these at edge)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }

    await next();
});

// Use Scalar API reference in development
app.UseScalarInDevelopment();

// Add global exception handler (must be early in pipeline)
app.UseExceptionHandler();
app.UseSerilogRequestLogging(options =>
{
    // Suppress health check pings from request logs
    options.GetLevel = (ctx, _, _) =>
        ctx.Request.Path.StartsWithSegments("/health") ? LogEventLevel.Verbose : LogEventLevel.Information;
});

app.UseCors("AllowFrontend");

// Only use HTTPS redirection if mTLS is not enabled (mTLS handles dual ports separately)
if (!mtlsConfig.Enabled)
{
    app.UseHttpsRedirection();
}

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map Health Check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

static Task WriteAuthErrorAsync(HttpResponse response, int statusCode, string message, string code)
{
    response.StatusCode = statusCode;
    response.ContentType = "application/json";

    var payload = JsonSerializer.Serialize(new
    {
        success = false,
        errors = new[]
        {
            new
            {
                message,
                code
            }
        }
    });

    return response.WriteAsync(payload);
}

// <summary>
// Run database migrations automatically
// </summary>
static async Task RunDatabaseMigrationsAsync(IServiceProvider services, ILogger<Program> logger)
{
    try
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IDbContext>();
        var migrations = await context.Database.GetPendingMigrationsAsync();

        if (migrations.Any())
        {
            logger.LogInformation("Found {MigrationCount} pending migration(s)", migrations.Count());
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed successfully");
        }
        else
        {
            logger.LogInformation("No pending migrations found");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error running database migrations");
        throw;
    }
}

// <summary>
// Run data seeders (environment-aware - runs different seeders based on environment)
// </summary>
static async Task RunDataSeedersAsync(IServiceProvider services, ILogger<Program> logger)
{
    try
    {
        using var scope = services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<DataSeederOrchestrator>();
        await orchestrator.SeedAllAsync();
        logger.LogInformation("Data seeding completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error running data seeders");
        throw;
    }
}

// <summary>
// Validate all infrastructure services are available before starting the application
// </summary>
static async Task ValidateServicesAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var healthCheckOrchestrator =
        scope.ServiceProvider.GetRequiredService<HealthCheckOrchestrator>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var allHealthy = await healthCheckOrchestrator.ValidateAllServicesAsync();

    if (!allHealthy)
    {
        logger.LogCritical("[FATAL] Application startup failed - required services are unavailable");
        logger.LogCritical("Please check your configuration and ensure all services are running.");
        Environment.Exit(1);
    }
}

/// <summary>
/// Partial class to expose Program for integration tests
/// </summary>
public partial class Program
{
}
