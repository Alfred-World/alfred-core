using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Common.Events;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// Default implementation of Unit of Work — exposes all repositories and manages transactions.
/// </summary>
public class DefaultUnitOfWork : IUnitOfWork
{
    private readonly IDbContext _context;
    private readonly IDomainEventDispatcher? _domainEventDispatcher;
    private readonly ICurrentUser? _currentUser;

    private IAccessRoleRepository? _accessRoles;
    private IAccessPermissionRepository? _accessPermissions;
    private IReferralCommissionSettingRepository? _referralCommissionSettings;
    private IReferralCommissionSettingHistoryRepository? _referralCommissionSettingHistories;
    private IProductRepository? _products;
    private IProductVariantRepository? _productVariants;
    private IMemberRepository? _members;
    private IReplicatedUserRepository? _replicatedUsers;
    private IAccountCloneRepository? _accountClones;
    private IAccountOrderRepository? _accountOrders;
    private ISourceAccountRepository? _sourceAccounts;
    private IAssetRepository? _assets;
    private IAssetLogRepository? _assetLogs;
    private IAttachmentRepository? _attachments;
    private IBrandRepository? _brands;
    private ICategoryRepository? _categories;
    private ICommodityRepository? _commodities;
    private IInvestmentTransactionRepository? _investmentTransactions;
    private IUnitRepository? _units;

    public DefaultUnitOfWork(
        IDbContext context,
        IDomainEventDispatcher? domainEventDispatcher = null,
        ICurrentUser? currentUser = null)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
        _currentUser = currentUser;
    }

    public IAccessRoleRepository AccessRoles =>
        _accessRoles ??= new AccessRoleRepository(_context);

    public IAccessPermissionRepository AccessPermissions =>
        _accessPermissions ??= new AccessPermissionRepository(_context);

    public IReferralCommissionSettingRepository ReferralCommissionSettings =>
        _referralCommissionSettings ??= new ReferralCommissionSettingRepository(_context);

    public IReferralCommissionSettingHistoryRepository ReferralCommissionSettingHistories =>
        _referralCommissionSettingHistories ??= new ReferralCommissionSettingHistoryRepository(_context);

    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    public IProductVariantRepository ProductVariants =>
        _productVariants ??= new ProductVariantRepository(_context);

    public IMemberRepository Members =>
        _members ??= new MemberRepository(_context);

    public IReplicatedUserRepository ReplicatedUsers =>
        _replicatedUsers ??= new ReplicatedUserRepository(_context);

    public IAccountCloneRepository AccountClones =>
        _accountClones ??= new AccountCloneRepository(_context);

    public IAccountOrderRepository AccountOrders =>
        _accountOrders ??= new AccountOrderRepository(_context);

    public ISourceAccountRepository SourceAccounts =>
        _sourceAccounts ??= new SourceAccountRepository(_context);

    public IAssetRepository Assets =>
        _assets ??= new AssetRepository(_context);

    public IAssetLogRepository AssetLogs =>
        _assetLogs ??= new AssetLogRepository(_context);

    public IAttachmentRepository Attachments =>
        _attachments ??= new AttachmentRepository(_context);

    public IBrandRepository Brands =>
        _brands ??= new BrandRepository(_context);

    public ICategoryRepository Categories =>
        _categories ??= new CategoryRepository(_context);

    public ICommodityRepository Commodities =>
        _commodities ??= new CommodityRepository(_context);

    public IInvestmentTransactionRepository InvestmentTransactions =>
        _investmentTransactions ??= new InvestmentTransactionRepository(_context);

    public IUnitRepository Units =>
        _units ??= new UnitRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_context is DbContext dbContext)
        {
            ApplyAuditFields(dbContext);

            var entitiesWithEvents = dbContext.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Where(entry => entry.Entity.DomainEvents.Count > 0)
                .Select(entry => entry.Entity)
                .ToList();

            var result = await dbContext.SaveChangesAsync(cancellationToken);

            if (_domainEventDispatcher != null && entitiesWithEvents.Count > 0)
            {
                var domainEvents = entitiesWithEvents
                    .SelectMany(entity => entity.DomainEvents)
                    .ToList();

                foreach (var entity in entitiesWithEvents)
                {
                    entity.ClearDomainEvents();
                }

                await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            }

            return result;
        }

        throw new InvalidOperationException("DbContext is not available");
    }

    private void ApplyAuditFields(DbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var currentUserId = _currentUser?.UserId;

        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            if (entry.Entity is not object)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is IHasCreationTime creationTime && creationTime.CreatedAt == default)
                {
                    creationTime.CreatedAt = now;
                }

                if (entry.Entity is IHasCreator creator && creator.CreatedById == null)
                {
                    creator.CreatedById = currentUserId;
                }

                continue;
            }

            if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is IHasDeletionTime deletionTime && deletionTime.IsDeleted)
                {
                    if (deletionTime.DeletedAt == null)
                    {
                        deletionTime.DeletedAt = now;
                    }

                    if (entry.Entity is IHasDeleter deleter && deleter.DeletedById == null)
                    {
                        deleter.DeletedById = currentUserId;
                    }

                    continue;
                }

                if (entry.Entity is IHasModificationTime modificationTime)
                {
                    modificationTime.UpdatedAt = now;
                }

                if (entry.Entity is IHasModifier modifier)
                {
                    modifier.UpdatedById = currentUserId;
                }

                continue;
            }

            if (entry.State == EntityState.Deleted && entry.Entity is IHasDeletionTime softDeleteEntity)
            {
                entry.State = EntityState.Modified;
                softDeleteEntity.IsDeleted = true;
                softDeleteEntity.DeletedAt = now;

                if (entry.Entity is IHasDeleter deleter)
                {
                    deleter.DeletedById = currentUserId;
                }
            }
        }
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        if (_context is not DbContext dbContext)
        {
            throw new InvalidOperationException("DbContext is not available");
        }

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public void Dispose()
    {
        if (_context is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
