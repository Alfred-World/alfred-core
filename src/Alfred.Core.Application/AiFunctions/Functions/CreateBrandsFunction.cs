using System.Text.Json;

using Alfred.Core.Application.Brands;
using Alfred.Core.Application.Brands.Dtos;
using Alfred.Core.Domain.Abstractions.Services.Ai;

using Microsoft.Extensions.Logging;

namespace Alfred.Core.Application.AiFunctions.Functions;

/// <summary>
/// AI-callable function that creates one or more brands in the system.
/// Example prompt: "Create 2 brands: Apple and Samsung"
/// The AI extracts brand details from the conversation and calls this function.
/// </summary>
public sealed class CreateBrandsFunction : IAiFunction
{
    private readonly IBrandService _brandService;
    private readonly ILogger<CreateBrandsFunction> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public CreateBrandsFunction(IBrandService brandService, ILogger<CreateBrandsFunction> logger)
    {
        _brandService = brandService;
        _logger = logger;
    }

    public string Name => "CreateBrands";

    public string Description =>
        "Create one or more new brands in the system. " +
        "Use when the user asks to create or add brands. " +
        "Example: 'Create brands Apple and Samsung', 'Add brand Nike with website nike.com'.";

    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            brands = new
            {
                type = "array",
                description = "List of brands to create",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new
                        {
                            type = "string",
                            description = "Brand name (required)"
                        },
                        website = new
                        {
                            type = "string",
                            description = "Official website URL (e.g. https://apple.com)"
                        },
                        support_phone = new
                        {
                            type = "string",
                            description = "Support phone number"
                        },
                        description = new
                        {
                            type = "string",
                            description = "Short description of the brand"
                        },
                        logo_url = new
                        {
                            type = "string",
                            description = "Brand logo URL"
                        }
                    },
                    required = new[] { "name" }
                }
            }
        },
        required = new[] { "brands" }
    };

    public async Task<AiFunctionResult> ExecuteAsync(
        string argumentsJson,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CreateBrands called with args: {Args}", argumentsJson);

        CreateBrandsArgs args;
        try
        {
            args = JsonSerializer.Deserialize<CreateBrandsArgs>(argumentsJson, JsonOptions)
                   ?? throw new JsonException("Deserialized value is null");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse CreateBrands arguments");
            return AiFunctionResult.Failure($"Invalid arguments: {ex.Message}");
        }

        if (args.Brands is not { Count: > 0 })
        {
            return AiFunctionResult.Failure("brands list is required and cannot be empty.");
        }

        var createdBrands = new List<string>();
        var failedBrands = new List<string>();

        foreach (var brand in args.Brands)
        {
            if (string.IsNullOrWhiteSpace(brand.Name))
            {
                failedBrands.Add("(empty name)");
                continue;
            }

            try
            {
                var dto = new CreateBrandDto(
                    brand.Name.Trim(),
                    brand.Website,
                    brand.SupportPhone,
                    brand.Description,
                    brand.LogoUrl,
                    null);

                var result = await _brandService.CreateBrandAsync(dto, cancellationToken);

                _logger.LogInformation("Brand created: {BrandName} (Id={BrandId})",
                    result.Name, result.Id);

                createdBrands.Add($"{result.Name} (Id: {result.Id})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create brand: {BrandName}", brand.Name);
                failedBrands.Add(brand.Name);
            }
        }

        if (failedBrands.Count > 0 && createdBrands.Count == 0)
        {
            return AiFunctionResult.Failure(
                $"Failed to create any brands. Errors: {string.Join(", ", failedBrands)}");
        }

        var message = $"Successfully created {createdBrands.Count} brand(s): {string.Join(", ", createdBrands)}";
        if (failedBrands.Count > 0)
        {
            message += $". Failed: {string.Join(", ", failedBrands)}";
        }

        return AiFunctionResult.Success(message, new { created = createdBrands, failed = failedBrands });
    }

    private sealed class CreateBrandsArgs
    {
        public List<BrandArgs>? Brands { get; set; }
    }

    private sealed class BrandArgs
    {
        public string Name { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? SupportPhone { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
    }
}
