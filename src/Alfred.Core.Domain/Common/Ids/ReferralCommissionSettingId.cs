namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct ReferralCommissionSettingId(Guid Value)
{
    public static ReferralCommissionSettingId New()
    {
        return new ReferralCommissionSettingId(Guid.CreateVersion7());
    }

    public static readonly ReferralCommissionSettingId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(ReferralCommissionSettingId id)
    {
        return id.Value;
    }

    public static implicit operator ReferralCommissionSettingId(Guid value)
    {
        return new ReferralCommissionSettingId(value);
    }
}
