using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class AccountClone : BaseEntity<AccountCloneId>, IHasCreationTime
{
    public ProductId ProductId { get; private set; }
    public SourceAccountId? SourceAccountId { get; private set; }
    public string ExternalAccountId { get; private set; } = string.Empty;
    public string Username { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public string? TwoFaSecret { get; private set; }
    public string? ExtraInfo { get; private set; }
    public AccountCloneStatus Status { get; private set; } = AccountCloneStatus.Init;
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; private set; }
    public DateTime? SoldAt { get; private set; }

    public Product? Product { get; private set; }
    public SourceAccount? SourceAccount { get; private set; }

    private AccountClone()
    {
        Id = AccountCloneId.New();
    }

    public static AccountClone Create(ProductId productId, string username, string password, string? twoFaSecret,
        string? extraInfo, string externalAccountId, SourceAccountId? sourceAccountId = null)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        if (string.IsNullOrWhiteSpace(externalAccountId))
        {
            throw new InvalidOperationException("External account id is required.");
        }

        return new AccountClone
        {
            ProductId = productId,
            SourceAccountId = sourceAccountId,
            ExternalAccountId = externalAccountId.Trim(),
            Username = username.Trim(),
            Password = password,
            TwoFaSecret = twoFaSecret,
            ExtraInfo = extraInfo,
            Status = AccountCloneStatus.Init,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void LinkToSourceAccount(SourceAccountId? sourceAccountId)
    {
        SourceAccountId = sourceAccountId;
    }

    public void MarkVerified()
    {
        ChangeReviewStatus(AccountCloneStatus.Verified);
    }

    public void MarkRejectedVerified()
    {
        ChangeReviewStatus(AccountCloneStatus.RejectVerified);
    }

    public void ChangeReviewStatus(AccountCloneStatus targetStatus)
    {
        if (targetStatus is AccountCloneStatus.Sold or AccountCloneStatus.InWarranty or AccountCloneStatus.Die)
        {
            throw new InvalidOperationException(
                $"Cannot manually change account clone to special status '{targetStatus}'.");
        }

        if (Status == targetStatus)
        {
            return;
        }

        // Init — reset from RejectVerified
        if (targetStatus == AccountCloneStatus.Init)
        {
            if (Status != AccountCloneStatus.RejectVerified)
            {
                throw new InvalidOperationException($"Cannot reset account clone to Init from status '{Status}'.");
            }

            Status = AccountCloneStatus.Init;
            return;
        }

        // Pending — submit for review (from Init or RejectVerified to re-submit)
        if (targetStatus == AccountCloneStatus.Pending)
        {
            if (Status != AccountCloneStatus.Init && Status != AccountCloneStatus.RejectVerified)
            {
                throw new InvalidOperationException($"Cannot submit account clone for review from status '{Status}'.");
            }

            Status = AccountCloneStatus.Pending;
            return;
        }

        // Verified — approve after review (must come from Pending)
        if (targetStatus == AccountCloneStatus.Verified)
        {
            if (Status != AccountCloneStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Cannot verify account clone from status '{Status}'. Clone must be Pending first.");
            }

            Status = AccountCloneStatus.Verified;
            VerifiedAt = DateTime.UtcNow;
            return;
        }

        // RejectVerified — reject after review (from Pending or Verified)
        if (targetStatus == AccountCloneStatus.RejectVerified)
        {
            if (Status != AccountCloneStatus.Pending && Status != AccountCloneStatus.Verified)
            {
                throw new InvalidOperationException($"Cannot reject verification from status '{Status}'.");
            }

            Status = AccountCloneStatus.RejectVerified;
            return;
        }

        throw new InvalidOperationException($"Manual status change to '{targetStatus}' is not supported.");
    }

    public void MarkSold(DateTime soldAtUtc)
    {
        if (Status != AccountCloneStatus.Verified)
        {
            throw new InvalidOperationException($"Cannot sell account clone from status '{Status}'.");
        }

        Status = AccountCloneStatus.Sold;
        SoldAt = soldAtUtc;
    }

    public void MarkInWarranty()
    {
        if (Status != AccountCloneStatus.Sold)
        {
            throw new InvalidOperationException($"Cannot mark in-warranty from status '{Status}'.");
        }

        Status = AccountCloneStatus.InWarranty;
    }

    public void MarkDie()
    {
        if (Status != AccountCloneStatus.InWarranty && Status != AccountCloneStatus.RejectVerified
                                                    && Status != AccountCloneStatus.Verified)
        {
            throw new InvalidOperationException($"Cannot mark die from status '{Status}'.");
        }

        Status = AccountCloneStatus.Die;
    }

    public void SetExternalAccountId(string externalAccountId)
    {
        if (string.IsNullOrWhiteSpace(externalAccountId))
        {
            throw new InvalidOperationException("External account id is required.");
        }

        ExternalAccountId = externalAccountId.Trim();
    }

    public void UpdateAccountInfo(string username, string password, string? twoFaSecret, string? extraInfo,
        string externalAccountId)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        Username = username.Trim();
        Password = password;
        TwoFaSecret = string.IsNullOrWhiteSpace(twoFaSecret) ? null : twoFaSecret.Trim();
        ExtraInfo = string.IsNullOrWhiteSpace(extraInfo) ? null : extraInfo.Trim();
        SetExternalAccountId(externalAccountId);
    }
}
