using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccountSales.Shared;

public sealed class MemberFieldMap : BaseFieldMap<Member>
{
    private static readonly Lazy<MemberFieldMap> _instance = new(() => new MemberFieldMap());

    private MemberFieldMap()
    {
    }

    public static MemberFieldMap Instance => _instance.Value;

    public override FieldMap<Member> Fields { get; } = new FieldMap<Member>()
        .Add("id", x => x.Id).AllowAll()
        .Add("displayName", x => x.DisplayName!).AllowAll()
        .Add("source", x => x.Source).AllowAll()
        .Add("sourceId", x => x.SourceId!).AllowAll()
        .Add("createdAt", x => x.CreatedAt).Sortable().Selectable();
}
