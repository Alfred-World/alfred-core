using Alfred.Core.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Alfred.Core.Infrastructure.Common.Converters;

public sealed class AssetIdConverter() : ValueConverter<AssetId, Guid>(id => id.Value, g => new AssetId(g));

public sealed class CategoryIdConverter() : ValueConverter<CategoryId, Guid>(id => id.Value, g => new CategoryId(g));

public sealed class BrandIdConverter() : ValueConverter<BrandId, Guid>(id => id.Value, g => new BrandId(g));

public sealed class UnitIdConverter() : ValueConverter<UnitId, Guid>(id => id.Value, g => new UnitId(g));

public sealed class AssetLogIdConverter() : ValueConverter<AssetLogId, Guid>(id => id.Value, g => new AssetLogId(g));

public sealed class AttachmentIdConverter()
    : ValueConverter<AttachmentId, Guid>(id => id.Value, g => new AttachmentId(g));

public sealed class CommodityIdConverter() : ValueConverter<CommodityId, Guid>(id => id.Value, g => new CommodityId(g));

public sealed class InvestmentTransactionIdConverter()
    : ValueConverter<InvestmentTransactionId, Guid>(id => id.Value, g => new InvestmentTransactionId(g));

public sealed class UrlConverter()
    : ValueConverter<Url, string>(u => u.Value, s => Url.Create(s));
