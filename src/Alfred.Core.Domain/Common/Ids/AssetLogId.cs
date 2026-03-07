namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct AssetLogId(Guid Value)
{
    public static AssetLogId New()
    {
        return new AssetLogId(Guid.CreateVersion7());
    }

    public static readonly AssetLogId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(AssetLogId id)
    {
        return id.Value;
    }

    public static implicit operator AssetLogId(Guid value)
    {
        return new AssetLogId(value);
    }
}
