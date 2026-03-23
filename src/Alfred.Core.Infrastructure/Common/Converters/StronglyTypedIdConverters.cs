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

public sealed class ProductIdConverter()
    : ValueConverter<ProductId, Guid>(id => id.Value, g => new ProductId(g));

public sealed class ProductVariantIdConverter()
    : ValueConverter<ProductVariantId, Guid>(id => id.Value, g => new ProductVariantId(g));

public sealed class MemberIdConverter()
    : ValueConverter<MemberId, Guid>(id => id.Value, g => new MemberId(g));

public sealed class AccountCloneIdConverter()
    : ValueConverter<AccountCloneId, Guid>(id => id.Value, g => new AccountCloneId(g));

public sealed class AccountOrderIdConverter()
    : ValueConverter<AccountOrderId, Guid>(id => id.Value, g => new AccountOrderId(g));

public sealed class AccessRoleIdConverter()
    : ValueConverter<AccessRoleId, Guid>(id => id.Value, g => new AccessRoleId(g));

public sealed class AccessPermissionIdConverter()
    : ValueConverter<AccessPermissionId, Guid>(id => id.Value, g => new AccessPermissionId(g));

public sealed class ReplicatedUserIdConverter()
    : ValueConverter<ReplicatedUserId, Guid>(id => id.Value, g => new ReplicatedUserId(g));

public sealed class ReferralCommissionSettingIdConverter()
    : ValueConverter<ReferralCommissionSettingId, Guid>(id => id.Value, g => new ReferralCommissionSettingId(g));

public sealed class ReferralCommissionSettingHistoryIdConverter()
    : ValueConverter<ReferralCommissionSettingHistoryId, Guid>(id => id.Value,
        g => new ReferralCommissionSettingHistoryId(g));

public sealed class UrlConverter()
    : ValueConverter<Url, string>(u => u.Value, s => Url.Create(s));
