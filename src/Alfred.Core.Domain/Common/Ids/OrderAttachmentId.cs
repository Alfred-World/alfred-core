namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct OrderAttachmentId(Guid Value)
{
    public static OrderAttachmentId New()
    {
        return new OrderAttachmentId(Guid.CreateVersion7());
    }

    public static readonly OrderAttachmentId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(OrderAttachmentId id)
    {
        return id.Value;
    }

    public static implicit operator OrderAttachmentId(Guid value)
    {
        return new OrderAttachmentId(value);
    }
}
