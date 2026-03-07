namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct AssetId(Guid Value)
{
    public static AssetId New()
    {
        return new AssetId(Guid.CreateVersion7());
    }

    public static readonly AssetId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(AssetId id)
    {
        return id.Value;
    }

    public static implicit operator AssetId(Guid value)
    {
        return new AssetId(value);
    }
}
