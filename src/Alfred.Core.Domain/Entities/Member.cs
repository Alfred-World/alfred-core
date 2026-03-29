using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class Member : BaseEntity<MemberId>, IHasCreationTime
{
    public string? DisplayName { get; private set; }
    public MemberSource Source { get; private set; } = MemberSource.Zalo;
    public string? SourceId { get; private set; }
    public string? CustomerNote { get; private set; }
    public DateTime CreatedAt { get; set; }

    private Member()
    {
        Id = MemberId.New();
    }

    public static Member Create(string? displayName, MemberSource source, string? sourceId, string? customerNote)
    {
        return new Member
        {
            DisplayName = displayName,
            Source = source,
            SourceId = sourceId,
            CustomerNote = customerNote,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string? displayName, MemberSource source, string? sourceId, string? customerNote)
    {
        DisplayName = displayName;
        Source = source;
        SourceId = sourceId;
        CustomerNote = customerNote;
    }
}
