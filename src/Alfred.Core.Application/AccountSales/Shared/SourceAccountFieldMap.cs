using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccountSales.Shared;

public sealed class SourceAccountFieldMap : BaseFieldMap<SourceAccount>
{
    private static readonly Lazy<SourceAccountFieldMap> _instance = new(() => new SourceAccountFieldMap());

    private SourceAccountFieldMap()
    {
    }

    public static SourceAccountFieldMap Instance => _instance.Value;

    public override FieldMap<SourceAccount> Fields { get; } = new FieldMap<SourceAccount>()
        .Add("id", x => x.Id).AllowAll()
        .Add("accountType", x => x.AccountType).AllowAll()
        .Add("username", x => x.Username).AllowAll()
        .Add("isActive", x => x.IsActive).AllowAll()
        .Add("createdAt", x => x.CreatedAt).AllowAll()
        .Add("updatedAt", x => x.UpdatedAt!).AllowAll();
}
