using Alfred.Core.Domain.Constants;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Common.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Alfred.Core.Infrastructure.Seeders;

/// <summary>
/// Seeds a default owner user and ensures Owner role has only system:* permission.
/// </summary>
public sealed class DefaultOwnerAccessSeeder : BaseDataSeeder
{
    private static readonly ReplicatedUserId HardcodedOwnerUserId =
        (ReplicatedUserId)new Guid("019d046f-2094-7e64-8caa-715ad7272a34");

    private const string HardcodedOwnerUserName = "owner";
    private const string HardcodedOwnerEmail = "owner@gmail.com";
    private const string HardcodedOwnerFullName = "System Owner";

    private const string OwnerRoleName = "Owner";

    private static readonly IReadOnlyList<(string Code, string Name, string Description)> PermissionCatalog =
    [
        (PermissionCodes.SystemAll, "Full System Access",
            "Complete administrative access to all system functions"),

        (PermissionCodes.AccessControl.AppRead, "Read Access Apps", "View access-control applications."),
        (PermissionCodes.AccessControl.UserRead, "Read Access Users", "View and search replicated access users."),
        (PermissionCodes.AccessControl.UserRoleUpdate, "Update Access User Roles",
            "Assign or revoke access roles for replicated users."),
        (PermissionCodes.AccessControl.RoleRead, "Read Access Roles", "View access roles and role details."),
        (PermissionCodes.AccessControl.RoleCreate, "Create Access Roles", "Create new access roles."),
        (PermissionCodes.AccessControl.RoleUpdate, "Update Access Roles", "Edit existing access roles."),
        (PermissionCodes.AccessControl.RoleDelete, "Delete Access Roles", "Delete access roles."),
        (PermissionCodes.AccessControl.PermissionRead, "Read Access Permissions", "View access permissions."),
        (PermissionCodes.AccessControl.RolePermissionUpdate, "Update Role Permissions",
            "Assign or remove permissions from roles."),

        (PermissionCodes.Brand.Read, "Read Brands", "View brands and brand details."),
        (PermissionCodes.Brand.Create, "Create Brands", "Create new brands."),
        (PermissionCodes.Brand.Update, "Update Brands", "Edit brand information."),
        (PermissionCodes.Brand.Delete, "Delete Brands", "Delete brands."),

        (PermissionCodes.Category.Read, "Read Categories", "View categories and category trees."),
        (PermissionCodes.Category.Create, "Create Categories", "Create new categories."),
        (PermissionCodes.Category.Update, "Update Categories", "Edit categories."),
        (PermissionCodes.Category.Delete, "Delete Categories", "Delete categories."),

        (PermissionCodes.Asset.Read, "Read Assets", "View assets and asset details."),
        (PermissionCodes.Asset.Create, "Create Assets", "Create new assets."),
        (PermissionCodes.Asset.Update, "Update Assets", "Edit assets."),
        (PermissionCodes.Asset.Delete, "Delete Assets", "Delete assets."),

        (PermissionCodes.AssetLog.Read, "Read Asset Logs", "View asset operational logs."),
        (PermissionCodes.AssetLog.Create, "Create Asset Logs", "Create asset operational logs."),
        (PermissionCodes.AssetLog.Delete, "Delete Asset Logs", "Delete asset operational logs."),

        (PermissionCodes.Commodity.Read, "Read Commodities", "View commodities and details."),
        (PermissionCodes.Commodity.Create, "Create Commodities", "Create new commodities."),
        (PermissionCodes.Commodity.Update, "Update Commodities", "Edit commodities."),
        (PermissionCodes.Commodity.Delete, "Delete Commodities", "Delete commodities."),

        (PermissionCodes.InvestmentTransaction.Read, "Read Investment Transactions", "View investment transactions."),
        (PermissionCodes.InvestmentTransaction.Create, "Create Investment Transactions",
            "Create investment transactions."),
        (PermissionCodes.InvestmentTransaction.Delete, "Delete Investment Transactions",
            "Delete investment transactions."),

        (PermissionCodes.Unit.Read, "Read Units", "View units and conversion trees."),
        (PermissionCodes.Unit.Create, "Create Units", "Create new units."),
        (PermissionCodes.Unit.Update, "Update Units", "Edit units."),
        (PermissionCodes.Unit.Delete, "Delete Units", "Delete units."),
        (PermissionCodes.Unit.Convert, "Convert Units", "Convert between units."),

        (PermissionCodes.Attachment.Read, "Read Attachments", "View attachments."),
        (PermissionCodes.Attachment.Create, "Create Attachments", "Upload and create attachments."),
        (PermissionCodes.Attachment.Delete, "Delete Attachments", "Delete attachments."),

        (PermissionCodes.File.Read, "Read Files", "Generate and use file download links."),
        (PermissionCodes.File.Create, "Create Files", "Generate upload links and upload files."),
        (PermissionCodes.File.Delete, "Delete Files", "Delete files from storage."),

        (PermissionCodes.AccountSales.ProductRead, "Read Account-Sales Products",
            "View products in account-sales module."),
        (PermissionCodes.AccountSales.ProductCreate, "Create Account-Sales Products",
            "Create products in account-sales module."),
        (PermissionCodes.AccountSales.ProductUpdate, "Update Account-Sales Products",
            "Edit products in account-sales module."),
        (PermissionCodes.AccountSales.MemberRead, "Read Account-Sales Members",
            "View and search members in account-sales module."),
        (PermissionCodes.AccountSales.MemberCreate, "Create Account-Sales Members",
            "Create members in account-sales module."),
        (PermissionCodes.AccountSales.AccountCloneRead, "Read Account Clones", "View account clones."),
        (PermissionCodes.AccountSales.AccountCloneCreate, "Create Account Clones", "Create account clones."),
        (PermissionCodes.AccountSales.AccountCloneReview, "Review Account Clones", "Review account clone requests."),
        (PermissionCodes.AccountSales.AccountCloneStatusUpdate, "Update Account Clone Status",
            "Update account clone status during review workflow."),
        (PermissionCodes.AccountSales.GithubUserRead, "Read Github Profiles", "Lookup Github profile data."),
        (PermissionCodes.AccountSales.WarrantyCheck, "Check Warranty", "Run warranty checks."),
        (PermissionCodes.AccountSales.RevenueRead, "Read Revenue", "View revenue by seller reports."),
        (PermissionCodes.AccountSales.CommissionSettingRead, "Read Referral Commission Setting",
            "View referral commission percentage setting."),
        (PermissionCodes.AccountSales.CommissionSettingHistoryRead, "Read Referral Commission History",
            "View referral commission percentage change history."),
        (PermissionCodes.AccountSales.CommissionSettingUpdate, "Update Referral Commission Setting",
            "Update referral commission percentage setting."),
        (PermissionCodes.AccountSales.OrderRead, "Read Orders", "View account orders."),
        (PermissionCodes.AccountSales.OrderSell, "Sell Orders", "Create account sales orders."),
        (PermissionCodes.AccountSales.OrderReplace, "Replace Orders", "Replace orders under warranty."),
        (PermissionCodes.AccountSales.SourceAccountRead, "Read Source Accounts",
            "View and search source accounts in account-sales module."),
        (PermissionCodes.AccountSales.SourceAccountCreate, "Create Source Accounts",
            "Create source accounts in account-sales module."),
        (PermissionCodes.AccountSales.SourceAccountUpdate, "Update Source Accounts",
            "Edit source accounts in account-sales module."),
        (PermissionCodes.AccountSales.SourceAccountDelete, "Delete Source Accounts",
            "Delete source accounts in account-sales module."),

        (PermissionCodes.AiChat.Send, "Use AI Chat", "Send prompts to AI assistant.")
    ];

    private readonly IDbContext _dbContext;

    public DefaultOwnerAccessSeeder(IDbContext dbContext, ILogger<DefaultOwnerAccessSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20260322191000_DefaultOwnerAccessSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var ownerRole = await EnsureOwnerRoleAsync(cancellationToken);
        var appPermissions = await EnsurePermissionsAsync(cancellationToken);

        var rolePermissionCount =
            await EnsureOwnerSystemPermissionOnlyAsync(ownerRole, appPermissions, cancellationToken);
        var assignedRole = await EnsureOwnerRoleAssignedToHardcodedUserAsync(ownerRole, cancellationToken);

        LogSuccess(
            $"Owner role ready (permissions:{rolePermissionCount}, hardcoded-user-role-assigned:{assignedRole.ToString().ToLowerInvariant()})");
    }

    private async Task<AccessRole> EnsureOwnerRoleAsync(CancellationToken cancellationToken)
    {
        var normalizedName = OwnerRoleName.ToUpperInvariant();

        var role = await _dbContext.Set<AccessRole>()
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedName, cancellationToken);

        if (role is not null)
        {
            return role;
        }

        role = AccessRole.Create(OwnerRoleName, isImmutable: true, isSystem: true);
        await _dbContext.Set<AccessRole>().AddAsync(role, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogDebug("Created owner role");
        return role;
    }

    private async Task<List<AccessPermission>> EnsurePermissionsAsync(CancellationToken cancellationToken)
    {
        var permissions = await _dbContext.Set<AccessPermission>()
            .ToListAsync(cancellationToken);

        var permissionByCode = permissions
            .ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

        var updatedPermissions = 0;

        foreach (var (code, name, description) in PermissionCatalog)
        {
            if (!permissionByCode.TryGetValue(code, out var existing))
            {
                continue;
            }

            var normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            if (string.Equals(existing.Name, name, StringComparison.Ordinal) &&
                string.Equals(existing.Description, normalizedDescription, StringComparison.Ordinal))
            {
                continue;
            }

            existing.Update(name, description, existing.IsActive);
            updatedPermissions++;
        }

        var existingCodes = permissions
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingPermissions = PermissionCatalog
            .Where(p => !existingCodes.Contains(p.Code))
            .Select(p => AccessPermission.Create(p.Code, p.Name, p.Description))
            .ToList();

        if (missingPermissions.Count > 0)
        {
            await _dbContext.Set<AccessPermission>().AddRangeAsync(missingPermissions, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            permissions.AddRange(missingPermissions);
            LogDebug($"Created {missingPermissions.Count} missing permissions");
        }

        if (updatedPermissions > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            LogDebug($"Updated {updatedPermissions} existing permissions to match catalog");
        }

        return permissions;
    }

    private async Task<int> EnsureOwnerSystemPermissionOnlyAsync(AccessRole role,
        IReadOnlyCollection<AccessPermission> permissions,
        CancellationToken cancellationToken)
    {
        var systemAllPermission = permissions
            .FirstOrDefault(x => string.Equals(x.Code, PermissionCodes.SystemAll, StringComparison.OrdinalIgnoreCase));

        if (systemAllPermission is null)
        {
            throw new InvalidOperationException(
                $"Required permission '{PermissionCodes.SystemAll}' was not found while seeding owner role.");
        }

        var existingRolePermissions = await _dbContext.Set<AccessRolePermission>()
            .Where(x => x.RoleId == role.Id)
            .ToListAsync(cancellationToken);

        var hasSystemAllPermission = existingRolePermissions
            .Any(x => x.PermissionId == systemAllPermission.Id);

        var staleRolePermissions = existingRolePermissions
            .Where(x => x.PermissionId != systemAllPermission.Id)
            .ToList();

        if (staleRolePermissions.Count > 0)
        {
            _dbContext.Set<AccessRolePermission>().RemoveRange(staleRolePermissions);
        }

        if (!hasSystemAllPermission)
        {
            var mapping = AccessRolePermission.Create(role.Id, systemAllPermission.Id);
            await _dbContext.Set<AccessRolePermission>().AddAsync(mapping, cancellationToken);
        }

        if (staleRolePermissions.Count > 0 || !hasSystemAllPermission)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            LogDebug(
                $"Synced owner role permissions (removed:{staleRolePermissions.Count}, addedSystemAll:{(!hasSystemAllPermission).ToString().ToLowerInvariant()})");
        }

        return 1;
    }

    private async Task<bool> EnsureOwnerRoleAssignedToHardcodedUserAsync(AccessRole ownerRole,
        CancellationToken cancellationToken)
    {
        var owner = await _dbContext.Set<ReplicatedUser>()
            .FirstOrDefaultAsync(x => x.Id == HardcodedOwnerUserId, cancellationToken);

        if (owner is null)
        {
            owner = ReplicatedUser.Create(
                HardcodedOwnerUserId,
                HardcodedOwnerUserName,
                HardcodedOwnerEmail,
                HardcodedOwnerFullName,
                null);
            await _dbContext.Set<ReplicatedUser>().AddAsync(owner, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            LogDebug($"Created hardcoded owner user in replicated_users: {HardcodedOwnerUserId}");
        }

        var hasRole = await _dbContext.Set<AccessUserRole>()
            .AnyAsync(x => x.UserId == HardcodedOwnerUserId && x.RoleId == ownerRole.Id, cancellationToken);

        if (hasRole)
        {
            return true;
        }

        var userRole = AccessUserRole.Create(HardcodedOwnerUserId, ownerRole.Id);
        await _dbContext.Set<AccessUserRole>().AddAsync(userRole, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogDebug($"Assigned owner role to hardcoded user: {HardcodedOwnerUserId}");
        return true;
    }
}
