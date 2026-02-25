using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Assets.Shared;

/// <summary>
/// Field map for AssetLog entity — defines fields available for filtering and sorting via DSL.
/// </summary>
public sealed class AssetLogFieldMap : BaseFieldMap<AssetLog>
{
    private static readonly Lazy<AssetLogFieldMap> _instance = new(() => new AssetLogFieldMap());

    private AssetLogFieldMap() { }

    public static AssetLogFieldMap Instance => _instance.Value;

    public override FieldMap<AssetLog> Fields { get; } = new FieldMap<AssetLog>()
        .Add("id", l => l.Id).AllowAll()
        .Add("assetId", l => l.AssetId).AllowAll()
        .Add("eventType", l => l.EventType).AllowAll()
        .Add("brandId", l => l.BrandId!).AllowAll()
        .Add("performedAt", l => l.PerformedAt).AllowAll()
        .Add("cost", l => l.Cost).AllowAll()
        .Add("nextDueDate", l => l.NextDueDate!).AllowAll()
        .Add("createdAt", l => l.CreatedAt).Sortable().Selectable();
}
