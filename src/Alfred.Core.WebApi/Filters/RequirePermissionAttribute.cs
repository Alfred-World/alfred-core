using System.Security.Claims;
using System.Text.Json;

using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Constants;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.WebApi.Contracts.Common;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.WebApi.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[] _requiredPermissions;

    public RequirePermissionAttribute(params string[] requiredPermissions)
    {
        _requiredPermissions = requiredPermissions
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var principal = context.HttpContext.User;
        if (principal.Identity?.IsAuthenticated != true)
        {
            context.Result =
                new UnauthorizedObjectResult(ApiErrorResponse.Unauthorized("Unauthorized", "UNAUTHORIZED"));
            return;
        }

        if (_requiredPermissions.Length == 0)
        {
            return;
        }

        var userId = ResolveUserId(principal);
        if (!userId.HasValue)
        {
            context.Result = new ObjectResult(ApiErrorResponse.Forbidden("Access denied", "FORBIDDEN"))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        var cache = context.HttpContext.RequestServices.GetRequiredService<ICacheProvider>();
        var cacheKey = $"core:perm:user:{userId.Value}";
        var cachedJson = await cache.GetAsync(cacheKey, context.HttpContext.RequestAborted);

        List<string> userPermissions;
        if (cachedJson is not null)
        {
            userPermissions = JsonSerializer.Deserialize<List<string>>(cachedJson) ?? [];
        }
        else
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<IDbContext>();
            userPermissions = await dbContext.Set<AccessUserRole>()
                .AsNoTracking()
                .Where(ur => ur.UserId == (ReplicatedUserId)userId.Value)
                .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Code))
                .Select(code => code.ToLower())
                .Distinct()
                .ToListAsync(context.HttpContext.RequestAborted);

            await cache.SetAsync(cacheKey, JsonSerializer.Serialize(userPermissions),
                TimeSpan.FromMinutes(5), context.HttpContext.RequestAborted);
        }

        var hasPermission = HasPermission(userPermissions);

        if (!hasPermission)
        {
            context.Result = new ObjectResult(ApiErrorResponse.Forbidden("Access denied", "FORBIDDEN"))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }

    private bool HasPermission(IReadOnlyCollection<string> permissions)
    {
        if (permissions.Count == 0)
        {
            return false;
        }

        return _requiredPermissions.Any(required =>
            permissions.Contains(required) ||
            permissions.Contains(PermissionCodes.SystemAll) ||
            permissions.Any(granted => MatchesWildcard(granted, required)));
    }

    private static bool MatchesWildcard(string grantedPermission, string requiredPermission)
    {
        if (grantedPermission == "*")
        {
            return true;
        }

        if (grantedPermission.EndsWith(":*", StringComparison.Ordinal))
        {
            var prefix = grantedPermission[..^2];
            return requiredPermission.StartsWith(prefix + ":", StringComparison.Ordinal);
        }

        return false;
    }

    private static Guid? ResolveUserId(ClaimsPrincipal principal)
    {
        var rawUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? principal.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(rawUserId))
        {
            return null;
        }

        if (Guid.TryParse(rawUserId, out var guidUserId) && guidUserId != Guid.Empty)
        {
            return guidUserId;
        }

        return null;
    }
}
