using System.ComponentModel;

using Alfred.Core.Domain.Querying;

using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Contracts.Common;

/// <summary>
/// Standard pagination query parameters for GET list endpoints.
/// For advanced filtering, use POST search endpoints with <see cref="SearchRequest{TFilter}"/>.
/// </summary>
public sealed record PaginationQueryParameters
{
    [FromQuery(Name = "page")]
    [DefaultValue(1)]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    [DefaultValue(20)]
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Sort expression (comma-separated).
    /// Example: "name,-createdAt" (ascending by name, descending by createdAt)
    /// </summary>
    [FromQuery(Name = "sort")]
    public string? Sort { get; init; }

    /// <summary>
    /// View name to determine which fields to return.
    /// </summary>
    [FromQuery(Name = "view")]
    public string? View { get; init; }
}

/// <summary>
/// Extension methods for converting query parameters to SearchRequest.
/// </summary>
public static class QueryParameterExtensions
{
    /// <summary>
    /// Convert PaginationQueryParameters to SearchRequest (no filter — use POST search for filtering).
    /// </summary>
    public static SearchRequest ToSearchRequest(this PaginationQueryParameters parameters)
    {
        return new SearchRequest
        {
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            Order = ParseSortString(parameters.Sort),
            View = parameters.View
        };
    }

    private static List<SortField>? ParseSortString(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return null;
        }

        var parts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var fields = new List<SortField>(parts.Length);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            if (trimmed.StartsWith('-'))
            {
                fields.Add(new SortField { Field = trimmed[1..], Direction = SortDirection.Desc });
            }
            else
            {
                fields.Add(new SortField { Field = trimmed, Direction = SortDirection.Asc });
            }
        }

        return fields.Count > 0 ? fields : null;
    }
}
