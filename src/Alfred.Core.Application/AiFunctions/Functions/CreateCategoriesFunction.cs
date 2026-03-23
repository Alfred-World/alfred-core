using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Alfred.Core.Application.Categories;
using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Domain.Abstractions.Services.Ai;
using Alfred.Core.Domain.Enums;

using Microsoft.Extensions.Logging;

namespace Alfred.Core.Application.AiFunctions.Functions;

/// <summary>
/// AI-callable function that creates one or more categories and sub-categories in the system.
/// Supports hierarchical creation: "a/1, a/2, b/1, b/2" or "a/1 and a/2 under category a"
/// Automatically creates parent categories if they don't exist.
/// </summary>
public sealed class CreateCategoriesFunction : IAiFunction
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CreateCategoriesFunction> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public CreateCategoriesFunction(ICategoryService categoryService, ILogger<CreateCategoriesFunction> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    public string Name => "CreateCategories";

    public string Description =>
        "Create one or more categories with optional hierarchical levels. " +
        "Support format: 'Electronics/Smartphones, Electronics/Tablets' to create Smartphones and Tablets under parent Electronics. " +
        "Parent categories are created automatically if they don't exist. " +
        "Example uses: Create Furniture/Chairs and Furniture/Desks, or Office/Monitors and Office/Keyboards.";

    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            categories = new
            {
                type = "array",
                description =
                    "List of category paths to create (e.g., [\"Electronics/Phones\", \"Electronics/Tablets\"])",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new
                        {
                            type = "string",
                            description =
                                "Category path like 'a/1' where 'a' is parent and '1' is name. Single level like 'a' is also allowed."
                        },
                        type = new
                        {
                            type = "string",
                            description = "Category type: 'Asset', 'Consumable', or 'Brand' (default: 'Asset')"
                        }
                    },
                    required = new[] { "path" }
                }
            }
        },
        required = new[] { "categories" }
    };

    public async Task<AiFunctionResult> ExecuteAsync(
        string argumentsJson,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var input = JsonSerializer.Deserialize<CreateCategoriesInput>(argumentsJson, JsonOptions)
                        ?? throw new ArgumentException("Invalid category input");

            if (input.Categories is not { Count: > 0 })
            {
                return AiFunctionResult.Failure("No categories provided");
            }

            var createdCategories = new List<string>();
            var failedCategories = new List<string>();
            var categoryCache = new Dictionary<string, Guid>(); // name → id for parent lookup

            // First pass: create all parents and collect them
            foreach (var catItem in input.Categories)
            {
                var parts = catItem.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    continue;
                }

                // If 2-level path, first part is parent
                if (parts.Length >= 2)
                {
                    var parentName = parts[0];
                    if (!categoryCache.ContainsKey(parentName))
                    {
                        try
                        {
                            // Parse type; default to Asset
                            var typeStr = catItem.Type ?? "Asset";
                            var type = Enum.TryParse<CategoryType>(typeStr, true, out var parsedType)
                                ? parsedType
                                : CategoryType.Asset;

                            var parentCode = GenerateSlug(parentName);
                            var parentDto = new CreateCategoryDto(
                                parentCode,
                                parentName,
                                null,
                                type,
                                null,
                                "[]");

                            var parentResult = await _categoryService.CreateCategoryAsync(parentDto, cancellationToken);
                            categoryCache[parentName] = parentResult.Id;
                            _logger.LogInformation("Created parent category: {ParentName} (Id={ParentId})",
                                parentName, parentResult.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to create parent category: {ParentName}", parentName);
                            // Skip if parent creation fails
                            continue;
                        }
                    }
                }
            }

            // Second pass: create all leaf categories
            foreach (var catItem in input.Categories)
            {
                try
                {
                    var parts = catItem.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0)
                    {
                        continue;
                    }

                    var categoryName = parts[^1]; // Last part is the category name
                    Guid? parentId = null;

                    // If 2+ level path, try to link to parent
                    if (parts.Length >= 2)
                    {
                        var parentName = parts[0];
                        if (categoryCache.TryGetValue(parentName, out var pId))
                        {
                            parentId = pId;
                        }
                        else
                        {
                            // Parent wasn't created, skip this category
                            _logger.LogWarning("Parent category '{ParentName}' not found for '{CategoryName}'",
                                parentName, categoryName);
                            failedCategories.Add(catItem.Path);
                            continue;
                        }
                    }

                    // Parse type; default to Asset
                    var typeStr = catItem.Type ?? "Asset";
                    var type = Enum.TryParse<CategoryType>(typeStr, true, out var parsedType)
                        ? parsedType
                        : CategoryType.Asset;

                    var categoryCode = GenerateSlug(categoryName);
                    var dto = new CreateCategoryDto(
                        categoryCode,
                        categoryName,
                        null,
                        type,
                        parentId,
                        "[]");

                    var result = await _categoryService.CreateCategoryAsync(dto, cancellationToken);
                    _logger.LogInformation("Category created: {CategoryName} (Id={CategoryId})",
                        result.Name, result.Id);

                    categoryCache[catItem.Path] = result.Id;
                    createdCategories.Add($"{result.Name} (Id: {result.Id})");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create category: {CategoryPath}", catItem.Path);
                    failedCategories.Add(catItem.Path);
                }
            }

            if (failedCategories.Count > 0 && createdCategories.Count == 0)
            {
                return AiFunctionResult.Failure(
                    $"Failed to create any categories. Errors: {string.Join(", ", failedCategories)}");
            }

            var message =
                $"Successfully created {createdCategories.Count} category(ies): {string.Join(", ", createdCategories)}";
            if (failedCategories.Count > 0)
            {
                message += $". Failed: {string.Join(", ", failedCategories)}";
            }

            return AiFunctionResult.Success(message, new { created = createdCategories, failed = failedCategories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateCategoriesFunction error");
            return AiFunctionResult.Failure($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate a code/slug from a category name.
    /// Example: "Đồ Gia Dụng" → "DO_GIA_DUNG"
    /// - Removes Vietnamese diacritics
    /// - Converts to uppercase
    /// - Replaces spaces with underscores
    /// - Removes special characters
    /// </summary>
    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "CATEGORY";
        }

        // Normalize Vietnamese characters (remove diacritics)
        var normalized = RemoveVietnameseDiacritics(input.Trim());

        // Convert to uppercase
        var upper = normalized.ToUpperInvariant();

        // Keep only alphanumeric and spaces, then replace spaces with underscores
        var sb = new StringBuilder();
        var lastWasSpace = false;

        foreach (var ch in upper)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                lastWasSpace = false;
            }
            else if (ch == ' ' && !lastWasSpace && sb.Length > 0)
            {
                sb.Append('_');
                lastWasSpace = true;
            }
        }

        var result = sb.ToString().Trim('_');

        // Collapse multiple underscores
        while (result.Contains("__"))
        {
            result = result.Replace("__", "_");
        }

        return string.IsNullOrEmpty(result) ? "CATEGORY" : result;
    }

    /// <summary>
    /// Remove Vietnamese diacritical marks from text.
    /// Maps: á→a, à→a, ả→a, ã→a, ạ→a, â→a, ấ→a, etc.
    /// </summary>
    private static string RemoveVietnameseDiacritics(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Vietnamese diacritic mappings
        var diacriticMap = new Dictionary<char, char>
        {
            // Lowercase a variants
            { 'á', 'a' }, { 'à', 'a' }, { 'ả', 'a' }, { 'ã', 'a' }, { 'ạ', 'a' },
            { 'ă', 'a' }, { 'ắ', 'a' }, { 'ằ', 'a' }, { 'ẳ', 'a' }, { 'ẵ', 'a' }, { 'ặ', 'a' },
            { 'â', 'a' }, { 'ấ', 'a' }, { 'ầ', 'a' }, { 'ẩ', 'a' }, { 'ẫ', 'a' }, { 'ậ', 'a' },

            // Lowercase e variants
            { 'é', 'e' }, { 'è', 'e' }, { 'ẻ', 'e' }, { 'ẽ', 'e' }, { 'ẹ', 'e' },
            { 'ê', 'e' }, { 'ế', 'e' }, { 'ề', 'e' }, { 'ể', 'e' }, { 'ễ', 'e' }, { 'ệ', 'e' },

            // Lowercase i variants
            { 'í', 'i' }, { 'ì', 'i' }, { 'ỉ', 'i' }, { 'ĩ', 'i' }, { 'ị', 'i' },

            // Lowercase o variants
            { 'ó', 'o' }, { 'ò', 'o' }, { 'ỏ', 'o' }, { 'õ', 'o' }, { 'ọ', 'o' },
            { 'ô', 'o' }, { 'ố', 'o' }, { 'ồ', 'o' }, { 'ổ', 'o' }, { 'ỗ', 'o' }, { 'ộ', 'o' },
            { 'ơ', 'o' }, { 'ớ', 'o' }, { 'ờ', 'o' }, { 'ở', 'o' }, { 'ỡ', 'o' }, { 'ợ', 'o' },

            // Lowercase u variants
            { 'ú', 'u' }, { 'ù', 'u' }, { 'ủ', 'u' }, { 'ũ', 'u' }, { 'ụ', 'u' },
            { 'ư', 'u' }, { 'ứ', 'u' }, { 'ừ', 'u' }, { 'ử', 'u' }, { 'ữ', 'u' }, { 'ự', 'u' },

            // Lowercase y variants
            { 'ý', 'y' }, { 'ỳ', 'y' }, { 'ỷ', 'y' }, { 'ỹ', 'y' }, { 'ỵ', 'y' },

            // Lowercase d
            { 'đ', 'd' },

            // Uppercase A variants
            { 'Á', 'A' }, { 'À', 'A' }, { 'Ả', 'A' }, { 'Ã', 'A' }, { 'Ạ', 'A' },
            { 'Ă', 'A' }, { 'Ắ', 'A' }, { 'Ằ', 'A' }, { 'Ẳ', 'A' }, { 'Ẵ', 'A' }, { 'Ặ', 'A' },
            { 'Â', 'A' }, { 'Ấ', 'A' }, { 'Ầ', 'A' }, { 'Ẩ', 'A' }, { 'Ẫ', 'A' }, { 'Ậ', 'A' },

            // Uppercase E variants
            { 'É', 'E' }, { 'È', 'E' }, { 'Ẻ', 'E' }, { 'Ẽ', 'E' }, { 'Ẹ', 'E' },
            { 'Ê', 'E' }, { 'Ế', 'E' }, { 'Ề', 'E' }, { 'Ể', 'E' }, { 'Ễ', 'E' }, { 'Ệ', 'E' },

            // Uppercase I variants
            { 'Í', 'I' }, { 'Ì', 'I' }, { 'Ỉ', 'I' }, { 'Ĩ', 'I' }, { 'Ị', 'I' },

            // Uppercase O variants
            { 'Ó', 'O' }, { 'Ò', 'O' }, { 'Ỏ', 'O' }, { 'Õ', 'O' }, { 'Ọ', 'O' },
            { 'Ô', 'O' }, { 'Ố', 'O' }, { 'Ồ', 'O' }, { 'Ổ', 'O' }, { 'Ỗ', 'O' }, { 'Ộ', 'O' },
            { 'Ơ', 'O' }, { 'Ớ', 'O' }, { 'Ờ', 'O' }, { 'Ở', 'O' }, { 'Ỡ', 'O' }, { 'Ợ', 'O' },

            // Uppercase U variants
            { 'Ú', 'U' }, { 'Ù', 'U' }, { 'Ủ', 'U' }, { 'Ũ', 'U' }, { 'Ụ', 'U' },
            { 'Ư', 'U' }, { 'Ứ', 'U' }, { 'Ừ', 'U' }, { 'Ử', 'U' }, { 'Ữ', 'U' }, { 'Ự', 'U' },

            // Uppercase Y variants
            { 'Ý', 'Y' }, { 'Ỳ', 'Y' }, { 'Ỷ', 'Y' }, { 'Ỹ', 'Y' }, { 'Ỵ', 'Y' },

            // Uppercase D
            { 'Đ', 'D' }
        };

        var sb = new StringBuilder();
        foreach (var ch in input)
        {
            if (diacriticMap.TryGetValue(ch, out var replacement))
            {
                sb.Append(replacement);
            }
            else
            {
                sb.Append(ch);
            }
        }

        return sb.ToString();
    }

    private record CreateCategoriesInput(
        [property: JsonPropertyName("categories")]
        List<CategoryPathItem>? Categories = null
    );

    private record CategoryPathItem(
        [property: JsonPropertyName("path")] string Path,
        [property: JsonPropertyName("type")] string? Type = null
    );
}
