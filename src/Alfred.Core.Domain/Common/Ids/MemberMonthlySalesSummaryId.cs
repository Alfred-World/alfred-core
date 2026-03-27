namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct MemberMonthlySalesSummaryId(Guid Value)
{
    public static MemberMonthlySalesSummaryId New()
    {
        return new MemberMonthlySalesSummaryId(Guid.CreateVersion7());
    }

    public static readonly MemberMonthlySalesSummaryId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(MemberMonthlySalesSummaryId id)
    {
        return id.Value;
    }

    public static implicit operator MemberMonthlySalesSummaryId(Guid value)
    {
        return new MemberMonthlySalesSummaryId(value);
    }
}
