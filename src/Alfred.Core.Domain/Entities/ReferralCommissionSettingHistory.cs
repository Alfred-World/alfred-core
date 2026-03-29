namespace Alfred.Core.Domain.Entities;

public sealed class ReferralCommissionSettingHistory : BaseEntity<ReferralCommissionSettingHistoryId>,
    IHasCreationTime
{
    public ReferralCommissionSettingId ReferralCommissionSettingId { get; private set; }
    public decimal PreviousCommissionPercent { get; private set; }
    public decimal NewCommissionPercent { get; private set; }
    public ReplicatedUserId? ChangedByUserId { get; private set; }
    public DateTime CreatedAt { get; set; }

    public ReferralCommissionSetting? ReferralCommissionSetting { get; private set; }
    public ReplicatedUser? ChangedByUser { get; private set; }

    private ReferralCommissionSettingHistory()
    {
        Id = ReferralCommissionSettingHistoryId.New();
    }

    public static ReferralCommissionSettingHistory Create(
        ReferralCommissionSettingId referralCommissionSettingId,
        decimal previousCommissionPercent,
        decimal newCommissionPercent,
        ReplicatedUserId? changedByUserId,
        DateTime changedAtUtc)
    {
        return new ReferralCommissionSettingHistory
        {
            ReferralCommissionSettingId = referralCommissionSettingId,
            PreviousCommissionPercent = NormalizePercent(previousCommissionPercent),
            NewCommissionPercent = NormalizePercent(newCommissionPercent),
            ChangedByUserId = changedByUserId,
            CreatedAt = changedAtUtc
        };
    }

    private static decimal NormalizePercent(decimal value)
    {
        return Math.Clamp(decimal.Round(value, 2, MidpointRounding.AwayFromZero), 0m, 100m);
    }
}
