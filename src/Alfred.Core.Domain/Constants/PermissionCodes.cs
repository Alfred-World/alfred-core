namespace Alfred.Core.Domain.Constants;

public static class PermissionCodes
{
    public const string SystemAll = "system:*";

    public static class AccessControl
    {
        public const string AppRead = "access-control:app:read";
        public const string UserRead = "access-control:user:read";
        public const string UserRoleUpdate = "access-control:user-role:update";
        public const string RoleRead = "access-control:role:read";
        public const string RoleCreate = "access-control:role:create";
        public const string RoleUpdate = "access-control:role:update";
        public const string RoleDelete = "access-control:role:delete";
        public const string PermissionRead = "access-control:permission:read";
        public const string RolePermissionUpdate = "access-control:role-permission:update";
    }

    public static class Brand
    {
        public const string Read = "brand:read";
        public const string Create = "brand:create";
        public const string Update = "brand:update";
        public const string Delete = "brand:delete";
    }

    public static class Category
    {
        public const string Read = "category:read";
        public const string Create = "category:create";
        public const string Update = "category:update";
        public const string Delete = "category:delete";
    }

    public static class Asset
    {
        public const string Read = "asset:read";
        public const string Create = "asset:create";
        public const string Update = "asset:update";
        public const string Delete = "asset:delete";
    }

    public static class AssetLog
    {
        public const string Read = "asset-log:read";
        public const string Create = "asset-log:create";
        public const string Delete = "asset-log:delete";
    }

    public static class Commodity
    {
        public const string Read = "commodity:read";
        public const string Create = "commodity:create";
        public const string Update = "commodity:update";
        public const string Delete = "commodity:delete";
    }

    public static class InvestmentTransaction
    {
        public const string Read = "investment-transaction:read";
        public const string Create = "investment-transaction:create";
        public const string Delete = "investment-transaction:delete";
    }

    public static class Unit
    {
        public const string Read = "unit:read";
        public const string Create = "unit:create";
        public const string Update = "unit:update";
        public const string Delete = "unit:delete";
        public const string Convert = "unit:convert";
    }

    public static class Attachment
    {
        public const string Read = "attachment:read";
        public const string Create = "attachment:create";
        public const string Delete = "attachment:delete";
    }

    public static class File
    {
        public const string Read = "file:read";
        public const string Create = "file:create";
        public const string Delete = "file:delete";
    }

    public static class AccountSales
    {
        public const string ProductRead = "account-sales:product:read";
        public const string ProductCreate = "account-sales:product:create";
        public const string ProductUpdate = "account-sales:product:update";
        public const string MemberRead = "account-sales:member:read";
        public const string MemberCreate = "account-sales:member:create";
        public const string MemberUpdate = "account-sales:member:update";
        public const string AccountCloneRead = "account-sales:account-clone:read";
        public const string AccountCloneCreate = "account-sales:account-clone:create";
        public const string AccountCloneReview = "account-sales:account-clone:review";
        public const string AccountCloneStatusUpdate = "account-sales:account-clone:status:update";
        public const string GithubUserRead = "account-sales:github-user:read";
        public const string WarrantyCheck = "account-sales:warranty:check";
        public const string RevenueRead = "account-sales:revenue:read";
        public const string CommissionSettingRead = "account-sales:commission-setting:read";
        public const string CommissionSettingHistoryRead = "account-sales:commission-setting:history:read";
        public const string CommissionSettingUpdate = "account-sales:commission-setting:update";
        public const string OrderRead = "account-sales:order:read";
        public const string OrderSell = "account-sales:order:sell";
        public const string OrderReplace = "account-sales:order:replace";
        public const string SourceAccountRead = "account-sales:source-account:read";
        public const string SourceAccountCreate = "account-sales:source-account:create";
        public const string SourceAccountUpdate = "account-sales:source-account:update";
        public const string SourceAccountDelete = "account-sales:source-account:delete";
        public const string OrderPaymentConfirm = "account-sales:order:payment:confirm";
        public const string OrderRefund = "account-sales:order:refund";
        public const string CommissionRead = "account-sales:commission:read";
        public const string CommissionPayout = "account-sales:commission:payout";
        public const string CommissionTransactionRead = "account-sales:commission:transaction:read";
        public const string BonusTierRead = "account-sales:bonus-tier:read";
        public const string BonusTierCreate = "account-sales:bonus-tier:create";
        public const string BonusTierUpdate = "account-sales:bonus-tier:update";
        public const string BonusTierDelete = "account-sales:bonus-tier:delete";
        public const string BonusSummaryRead = "account-sales:bonus-summary:read";
        public const string BonusSummaryProgressRead = "account-sales:bonus-summary:progress:read";
        public const string BonusTransactionRead = "account-sales:bonus-transaction:read";
        public const string BonusTransactionPay = "account-sales:bonus-transaction:pay";
        public const string BonusTransactionCancel = "account-sales:bonus-transaction:cancel";
    }

    public static class AiChat
    {
        public const string Send = "ai-chat:send";
    }
}
