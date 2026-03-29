using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccountSales.Shared;

public sealed class AccountCloneFieldMap : BaseFieldMap<AccountClone>
{
    private static readonly Lazy<AccountCloneFieldMap> _instance = new(() => new AccountCloneFieldMap());

    private AccountCloneFieldMap()
    {
    }

    public static AccountCloneFieldMap Instance => _instance.Value;

    public override FieldMap<AccountClone> Fields { get; } = new FieldMap<AccountClone>()
        .Add("id", x => x.Id).AllowAll()
        .Add("productId", x => x.ProductId).AllowAll()
        .Add("productName", x => x.Product!.Name).AllowAll()
        .Add("externalAccountId", x => x.ExternalAccountId).AllowAll()
        .Add("username", x => x.Username).AllowAll()
        .Add("status", x => x.Status).AllowAll()
        .Add("createdAt", x => x.CreatedAt).AllowAll()
        .Add("soldAt", x => x.SoldAt!).AllowAll();
}
