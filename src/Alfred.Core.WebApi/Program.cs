using System.Security.Cryptography.X509Certificates;
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

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;

using Serilog;

// Load environment variables from .env file
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
DotEnvLoader.LoadForEnvironment(environment);

// Load and validate configuration
AppConfiguration appConfig = new();
MtlsConfiguration mtlsConfig = new();

var builder = WebApplication.CreateBuilder(args);

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

// Add Infrastructure layer (Database)
builder.Services.AddInfrastructure();

// Add Cookie Authentication for SSO
var ssoCookieDomain = Environment.GetEnvironmentVariable("SSO_COOKIE_DOMAIN");
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AlfredSession";
        // Cookie domain is NOT set explicitly because:
        // 1. Request comes to localhost (via YARP reverse proxy)
        // 2. ASP.NET refuses to set cookie for different domain than request host
        // Instead, we rely on ForwardedHeaders middleware to detect the correct host
        // and the cookie will be set for that host (gateway.test when behind YARP)
        // 
        // For cross-subdomain sharing in production (e.g., *.alfred.com),
        // configure ForwardedHeaders properly and consider using cookie path/domain options
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None; // Allow cross-origin cookie setting
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Required for SameSite=None
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        // For API-based auth, return 401 instead of redirect
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });

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

// Use Scalar API reference in development
app.UseScalarInDevelopment();

// Add global exception handler (must be early in pipeline)
app.UseExceptionHandler();
app.UseSerilogRequestLogging();

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
