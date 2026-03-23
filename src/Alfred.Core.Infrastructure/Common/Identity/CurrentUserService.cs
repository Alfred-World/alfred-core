using System.Security.Claims;

using Alfred.Core.Domain.Abstractions;

using Microsoft.AspNetCore.Http;

namespace Alfred.Core.Infrastructure.Common.Identity;

/// <summary>
/// Resolves current user details from HTTP claims.
/// </summary>
public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = GetClaimValue(ClaimTypes.NameIdentifier)
                              ?? GetClaimValue("sub");

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Username => GetClaimValue(ClaimTypes.Name)
                               ?? GetClaimValue("preferred_username");

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    private string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }
}
