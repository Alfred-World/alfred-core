using Alfred.Core.Domain.Common.Base;

namespace Alfred.Core.Domain.Entities;

/// <summary>
/// Local replicated snapshot of Identity user profile for read-heavy Core queries.
/// </summary>
public sealed class ReplicatedUser : BaseEntity<ReplicatedUserId>
{
    public string UserName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? FullName { get; private set; }
    public string? Avatar { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<AccessUserRole> UserRoles { get; private set; } = new List<AccessUserRole>();

    private ReplicatedUser()
    {
    }

    public static ReplicatedUser Create(ReplicatedUserId identityUserId, string userName, string email,
        string? fullName,
        string? avatar)
    {
        if (identityUserId == ReplicatedUserId.Empty)
        {
            throw new InvalidOperationException("Identity user id must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        var now = DateTime.UtcNow;

        return new ReplicatedUser
        {
            Id = identityUserId,
            UserName = userName.Trim(),
            Email = email.Trim(),
            FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim(),
            Avatar = string.IsNullOrWhiteSpace(avatar) ? null : avatar.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateProfile(string userName, string email, string? fullName, string? avatar)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        UserName = userName.Trim();
        Email = email.Trim();
        FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();
        Avatar = string.IsNullOrWhiteSpace(avatar) ? null : avatar.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
