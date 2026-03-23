namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct ReferralCommissionSettingHistoryId(Guid Value)
{
    public static ReferralCommissionSettingHistoryId New()
    {
        return new ReferralCommissionSettingHistoryId(Guid.CreateVersion7());
    }

    public static readonly ReferralCommissionSettingHistoryId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(ReferralCommissionSettingHistoryId id)
    {
        return id.Value;
    }

    public static implicit operator ReferralCommissionSettingHistoryId(Guid value)
    {
        return new ReferralCommissionSettingHistoryId(value);
    }
}
