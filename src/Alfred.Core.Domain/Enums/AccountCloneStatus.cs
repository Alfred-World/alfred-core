namespace Alfred.Core.Domain.Enums;

public enum AccountCloneStatus
{
    /// <summary>Initial state when a clone is first created.</summary>
    Init = 1,

    /// <summary>Approved and eligible for selling.</summary>
    Verified = 2,

    /// <summary>System-set when sold via the sell flow.</summary>
    Sold = 3,

    /// <summary>System-set when under active warranty.</summary>
    InWarranty = 4,

    /// <summary>System-set terminal state (account died / expired).</summary>
    Die = 5,

    /// <summary>Verification was rejected; can be re-submitted to Pending.</summary>
    RejectVerified = 6,

    /// <summary>Submitted for review; awaiting verification (typically ~3 business days).</summary>
    Pending = 7
}
