namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct CommissionId(Guid Value)
{
    public static CommissionId New()
    {
        return new CommissionId(Guid.CreateVersion7());
    }

    public static readonly CommissionId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(CommissionId id)
    {
        return id.Value;
    }

    public static implicit operator CommissionId(Guid value)
    {
        return new CommissionId(value);
    }
}
