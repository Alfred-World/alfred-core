using System.Security.Claims;

namespace Alfred.Core.Domain.Abstractions;

/// <summary>
/// Provides access to the current authenticated user information.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
    ClaimsPrincipal? Principal { get; }
}
