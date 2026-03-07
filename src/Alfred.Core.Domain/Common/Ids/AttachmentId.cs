namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct AttachmentId(Guid Value)
{
    public static AttachmentId New()
    {
        return new AttachmentId(Guid.CreateVersion7());
    }

    public static readonly AttachmentId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(AttachmentId id)
    {
        return id.Value;
    }

    public static implicit operator AttachmentId(Guid value)
    {
        return new AttachmentId(value);
    }
}
