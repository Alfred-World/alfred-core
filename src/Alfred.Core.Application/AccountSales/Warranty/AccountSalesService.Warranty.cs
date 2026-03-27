using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.AccountSales.Shared;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales;

public sealed partial class AccountSalesService
{
    private static readonly HttpClient GithubHttpClient = CreateGithubHttpClient();

    public async Task<GithubUserProfileDto> GetGithubUserProfileAsync(string username,
        CancellationToken cancellationToken = default)
    {
        var normalized = (username ?? string.Empty).Trim().TrimStart('@');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Github username is required.");
        }

        var response = await GithubHttpClient.GetAsync($"users/{Uri.EscapeDataString(normalized)}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException($"Github user '{normalized}' not found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Github API returned status {(int)response.StatusCode}.");
        }

        var data = await response.Content.ReadFromJsonAsync<GithubUserApiResponse>(cancellationToken);
        if (data is null)
        {
            throw new InvalidOperationException("Unable to parse Github API response.");
        }

        return new GithubUserProfileDto(
            data.Login,
            data.Id,
            data.NodeId,
            data.AvatarUrl,
            data.HtmlUrl,
            data.Name,
            data.Company,
            data.Blog,
            data.Location,
            data.Email,
            data.Bio,
            data.PublicRepos,
            data.Followers,
            data.Following,
            data.CreatedAt,
            data.UpdatedAt);
    }

    public async Task<WarrantyCheckResultDto> CheckWarrantyAsync(CheckWarrantyDto dto,
        CancellationToken cancellationToken = default)
    {
        var username = (dto.Username ?? string.Empty).Trim();
        var externalAccountId = string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Username is required for warranty check.");
        }

        if (dto.ProductId.HasValue && !string.IsNullOrWhiteSpace(username))
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId.Value, cancellationToken);
            if (product != null && product.ProductType == AccountProductType.Github)
            {
                var githubProfile = await GetGithubUserProfileAsync(username, cancellationToken);
                externalAccountId = githubProfile.Id.ToString();
                username = string.Empty;
            }
        }

        var query = _unitOfWork.AccountOrders
            .GetQueryable([o => o.Member!, o => o.ReferrerMember!, o => o.Product!, o => o.AccountClone!])
            .Where(o =>
                (string.IsNullOrWhiteSpace(username) || o.AccountClone!.Username == username) &&
                (string.IsNullOrWhiteSpace(externalAccountId) ||
                 o.AccountClone!.ExternalAccountId == externalAccountId) &&
                (!dto.ProductId.HasValue || o.ProductId == dto.ProductId.Value))
            .OrderByDescending(o => o.PurchaseDate);

        var order = await _executor.FirstOrDefaultAsync(query, cancellationToken);
        if (order is null)
        {
            return new WarrantyCheckResultDto(false, false, "Account was not sold by our system.", null);
        }

        var inWarranty = order.Status == AccountOrderStatus.Active && order.WarrantyExpiry >= DateTime.UtcNow;

        return new WarrantyCheckResultDto(
            true,
            inWarranty,
            inWarranty ? "Account is in warranty window." : "Account exists but warranty has expired.",
            order.ToDto());
    }

    private sealed record GithubUserApiResponse(
        [property: JsonPropertyName("login")] string Login,
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("node_id")]
        string NodeId,
        [property: JsonPropertyName("avatar_url")]
        string AvatarUrl,
        [property: JsonPropertyName("html_url")]
        string HtmlUrl,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("company")]
        string? Company,
        [property: JsonPropertyName("blog")] string? Blog,
        [property: JsonPropertyName("location")]
        string? Location,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("bio")] string? Bio,
        [property: JsonPropertyName("public_repos")]
        int PublicRepos,
        [property: JsonPropertyName("followers")]
        int Followers,
        [property: JsonPropertyName("following")]
        int Following,
        [property: JsonPropertyName("created_at")]
        DateTime CreatedAt,
        [property: JsonPropertyName("updated_at")]
        DateTime UpdatedAt);

    private static HttpClient CreateGithubHttpClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://api.github.com/"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("alfred-core", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        return client;
    }
}
