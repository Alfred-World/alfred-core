namespace Alfred.Core.Domain.Entities;

public sealed class ReferralCommissionSetting : BaseEntity<ReferralCommissionSettingId>, IHasCreationTime,
    IHasModificationTime
{
    public decimal CommissionPercent { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ReferralCommissionSettingHistory> Histories { get; private set; } =
        new List<ReferralCommissionSettingHistory>();

    private ReferralCommissionSetting()
    {
        Id = ReferralCommissionSettingId.New();
    }

    public static ReferralCommissionSetting Create(decimal commissionPercent)
    {
        return new ReferralCommissionSetting
        {
            CommissionPercent = NormalizePercent(commissionPercent),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePercent(decimal commissionPercent)
    {
        CommissionPercent = NormalizePercent(commissionPercent);
        UpdatedAt = DateTime.UtcNow;
    }

    private static decimal NormalizePercent(decimal value)
    {
        return Math.Clamp(decimal.Round(value, 2, MidpointRounding.AwayFromZero), 0m, 100m);
    }
}
