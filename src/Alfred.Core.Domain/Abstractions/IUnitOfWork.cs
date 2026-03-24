using Alfred.Core.Domain.Abstractions;

/// <summary>
/// Unit of Work pattern — single gateway for all repositories and transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IAccessRoleRepository AccessRoles { get; }
    IAccessPermissionRepository AccessPermissions { get; }
    IReferralCommissionSettingRepository ReferralCommissionSettings { get; }
    IReferralCommissionSettingHistoryRepository ReferralCommissionSettingHistories { get; }
    IProductRepository Products { get; }
    IProductVariantRepository ProductVariants { get; }
    IMemberRepository Members { get; }
    IReplicatedUserRepository ReplicatedUsers { get; }
    IAccountCloneRepository AccountClones { get; }
    IAccountOrderRepository AccountOrders { get; }
    ISourceAccountRepository SourceAccounts { get; }
    IAssetRepository Assets { get; }
    IAssetLogRepository AssetLogs { get; }
    IAttachmentRepository Attachments { get; }
    IBrandRepository Brands { get; }
    ICategoryRepository Categories { get; }
    ICommodityRepository Commodities { get; }
    IInvestmentTransactionRepository InvestmentTransactions { get; }
    IUnitRepository Units { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
}
