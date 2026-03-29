using System.Linq.Expressions;

using Alfred.Core.Application.AccountSales.Dtos;
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
        .Add("password", x => x.Password).Selectable()
        .Add("twoFaSecret", x => x.TwoFaSecret!).Selectable()
        .Add("recoveryEmail", x => x.RecoveryEmail!).Selectable()
        .Add("recoveryPhone", x => x.RecoveryPhone!).Selectable()
        .Add("notes", x => x.Notes!).Selectable()
        .Add("cloneCount", x => x.Clones.Count()).Selectable()
        .Add("createdAt", x => x.CreatedAt).AllowAll()
        .Add("updatedAt", x => x.UpdatedAt!).AllowAll();

    public static ViewRegistry<SourceAccount, SourceAccountDto> Views { get; } =
        new ViewRegistry<SourceAccount, SourceAccountDto>()
            .Register("list", new Expression<Func<SourceAccountDto, object?>>[]
            {
                x => x.Id,
                x => x.AccountType,
                x => x.Username,
                x => x.IsActive,
                x => x.CloneCount,
                x => x.CreatedAt,
                x => x.UpdatedAt
            })
            .Register("detail", new Expression<Func<SourceAccountDto, object?>>[]
            {
                x => x.Id,
                x => x.AccountType,
                x => x.Username,
                x => x.Password,
                x => x.TwoFaSecret,
                x => x.RecoveryEmail,
                x => x.RecoveryPhone,
                x => x.Notes,
                x => x.IsActive,
                x => x.CloneCount,
                x => x.CreatedAt,
                x => x.UpdatedAt
            })
            .SetDefault("list");
}
