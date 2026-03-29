using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

/// <summary>
/// Represents an original/root account (e.g. edu email, Microsoft, Google, iCloud account)
/// used to create or verify clone accounts for sale.
/// </summary>
public sealed class SourceAccount : BaseEntity<SourceAccountId>, IHasCreationTime
{
    public AccountProductType AccountType { get; private set; }
    public string Username { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public string? TwoFaSecret { get; private set; }
    public string? RecoveryEmail { get; private set; }
    public string? RecoveryPhone { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<AccountClone> Clones => _clones.AsReadOnly();
    private readonly List<AccountClone> _clones = [];

    private SourceAccount()
    {
        Id = SourceAccountId.New();
    }

    public static SourceAccount Create(
        AccountProductType accountType,
        string username,
        string password,
        string? twoFaSecret,
        string? recoveryEmail,
        string? recoveryPhone,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        return new SourceAccount
        {
            AccountType = accountType,
            Username = username.Trim(),
            Password = password,
            TwoFaSecret = twoFaSecret,
            RecoveryEmail = recoveryEmail?.Trim(),
            RecoveryPhone = recoveryPhone?.Trim(),
            Notes = notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        AccountProductType accountType,
        string username,
        string password,
        string? twoFaSecret,
        string? recoveryEmail,
        string? recoveryPhone,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        AccountType = accountType;
        Username = username.Trim();
        Password = password;
        TwoFaSecret = twoFaSecret;
        RecoveryEmail = recoveryEmail?.Trim();
        RecoveryPhone = recoveryPhone?.Trim();
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
