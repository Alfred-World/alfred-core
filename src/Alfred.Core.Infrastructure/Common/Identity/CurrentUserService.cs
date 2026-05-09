using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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

            return TryParseUserId(userIdClaim);
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

    private static Guid? TryParseUserId(string? rawUserId)
    {
        if (string.IsNullOrWhiteSpace(rawUserId))
        {
            return null;
        }

        if (Guid.TryParse(rawUserId, out var guidUserId) && guidUserId != Guid.Empty)
        {
            return guidUserId;
        }

        if (!long.TryParse(rawUserId, out var legacyUserId) || legacyUserId <= 0)
        {
            return null;
        }

        return MapLegacyInt64ToGuid(legacyUserId);
    }

    private static Guid MapLegacyInt64ToGuid(long userId)
    {
        var raw = Encoding.UTF8.GetBytes($"legacy-user:{userId}");
        var hash = MD5.HashData(raw);
        return new Guid(hash);
    }
}
